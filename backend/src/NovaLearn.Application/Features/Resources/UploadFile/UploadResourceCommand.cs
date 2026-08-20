using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Resources.UploadFile;

/// <summary>
/// Posts an uploaded file to the wall.
///
/// The stream is handed over rather than the whole file in memory, so a large video is written
/// straight through to storage.
/// </summary>
public sealed record UploadResourceCommand(
    string Title,
    string? Description,
    Guid? CourseId,
    string FileName,
    long? DeclaredSizeBytes,
    Stream Content)
    : IRequest<Result<ResourceDto>>;

public sealed class UploadResourceCommandHandler(
    IResourceRepository resources,
    ICourseRepository courses,
    IFileStorage storage,
    IUploadLimits limits,
    ISettingsProvider settings,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadResourceCommand, Result<ResourceDto>>
{
    public async Task<Result<ResourceDto>> Handle(
        UploadResourceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<ResourceDto>(ResourceErrors.Unauthenticated);
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result.Failure<ResourceDto>(
                Error.Validation("resource.title_required", "A resource needs a title."));
        }

        if (request.DeclaredSizeBytes is 0)
        {
            return Result.Failure<ResourceDto>(ResourceErrors.EmptyFile);
        }

        // The effective ceiling is the tighter of two numbers: the request pipeline's own hard
        // limit, fixed at startup and unrelated to any setting, and the admin-configured business
        // limit, which can move at any time. An admin raising the setting can never promise more
        // than the pipeline actually allows through.
        PlatformSettingsSnapshot platform = await settings.GetAsync(cancellationToken);
        int effectiveMaxMegabytes = Math.Min(platform.MaxUploadSizeMb, limits.MaxFileSizeMegabytes);
        long effectiveMaxBytes = effectiveMaxMegabytes * 1024L * 1024L;

        // Checked before a byte is written. The request pipeline caps the body as well, so this is
        // about giving a clear answer rather than about being the only line of defence.
        if (request.DeclaredSizeBytes > effectiveMaxBytes)
        {
            return Result.Failure<ResourceDto>(ResourceErrors.FileTooLarge(effectiveMaxMegabytes));
        }

        // The kind and the content type come from the extension, never from what the browser said
        // the file was. We are the ones serving these bytes back later.
        if (!UploadedFileTypes.TryResolve(request.FileName, out ResourceKind kind, out string contentType))
        {
            return Result.Failure<ResourceDto>(ResourceErrors.UnsupportedFileType);
        }

        if (request.CourseId is { } courseId)
        {
            Course? course = await courses.GetByIdAsync(courseId, cancellationToken);

            if (course is null)
            {
                return Result.Failure<ResourceDto>(ResourceErrors.CourseNotFound);
            }

            if (!ResourceAuthority.CanPostToCourse(course, currentUser))
            {
                return Result.Failure<ResourceDto>(ResourceErrors.NotCourseOwner);
            }
        }

        StoredFile stored = await storage.SaveAsync(
            request.Content, request.FileName, cancellationToken);

        if (stored.SizeBytes == 0)
        {
            await storage.DeleteAsync(stored.Key, cancellationToken);
            return Result.Failure<ResourceDto>(ResourceErrors.EmptyFile);
        }

        var resource = Resource.ForUpload(
            request.Title,
            request.Description,
            kind,
            stored.Key,
            Path.GetFileName(request.FileName),
            contentType,
            stored.SizeBytes,
            request.CourseId,
            callerId);

        try
        {
            await resources.AddAsync(resource, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Without this the file would survive as an orphan nobody can reach or clean up.
            await storage.DeleteAsync(stored.Key, cancellationToken);
            throw;
        }

        Resource saved = await resources.GetByIdAsync(resource.Id, cancellationToken) ?? resource;

        return Result.Success(ResourceMapper.ToDto(saved, canManage: true));
    }
}
