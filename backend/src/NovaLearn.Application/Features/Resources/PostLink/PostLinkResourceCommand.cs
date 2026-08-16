using FluentValidation;
using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Resources.PostLink;

/// <summary>
/// Posts an external address to the wall: YouTube, Drive, or anything else on the web.
/// </summary>
public sealed record PostLinkResourceCommand(
    string Title, string? Description, string Url, Guid? CourseId)
    : IRequest<Result<ResourceDto>>;

public sealed class PostLinkResourceCommandValidator : AbstractValidator<PostLinkResourceCommand>
{
    public PostLinkResourceCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(Resource.TitleMaxLength);

        RuleFor(command => command.Description)
            .MaximumLength(Resource.DescriptionMaxLength);

        RuleFor(command => command.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(ResourceAddress.IsUsable!)
            .WithMessage("A link must be an absolute http or https address.");
    }
}

public sealed class PostLinkResourceCommandHandler(
    IResourceRepository resources,
    ICourseRepository courses,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PostLinkResourceCommand, Result<ResourceDto>>
{
    public async Task<Result<ResourceDto>> Handle(
        PostLinkResourceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } callerId)
        {
            return Result.Failure<ResourceDto>(ResourceErrors.Unauthenticated);
        }

        if (!ResourceAddress.IsUsable(request.Url))
        {
            return Result.Failure<ResourceDto>(ResourceErrors.InvalidLink);
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

        var resource = Resource.ForLink(
            request.Title, request.Description, request.Url, request.CourseId, callerId);

        await resources.AddAsync(resource, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-read so the response carries the course title and the poster's name, which setting a
        // foreign key does not populate. The course slice learned this the hard way.
        Resource saved = await resources.GetByIdAsync(resource.Id, cancellationToken) ?? resource;

        return Result.Success(ResourceMapper.ToDto(saved, canManage: true));
    }
}
