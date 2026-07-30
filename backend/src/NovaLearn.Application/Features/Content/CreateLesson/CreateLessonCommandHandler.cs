using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.CreateLesson;

public sealed class CreateLessonCommandHandler(
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateLessonCommand, Result<LessonDto>>
{
    public async Task<Result<LessonDto>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        CourseModule? module = await content.GetModuleByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Result.Failure<LessonDto>(ContentErrors.ModuleNotFound);
        }

        if (ContentAuthority.CheckCanManage(module.Course, currentUser) is Error error)
        {
            return Result.Failure<LessonDto>(error);
        }

        int sortOrder = await content.NextLessonSortOrderAsync(request.ModuleId, cancellationToken);

        // The module is the aggregate root, so it creates and owns the new lesson.
        Lesson lesson = module.AddLesson(
            request.Title,
            request.Type,
            request.ContentUrl,
            request.TextContent,
            request.DurationMinutes,
            sortOrder,
            request.IsPreview);

        // The module owns the lesson, but the insert still has to be stated explicitly; see
        // ICourseContentRepository.AddLessonAsync for why.
        await content.AddLessonAsync(lesson, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return LessonDto.FromEntity(lesson);
    }
}
