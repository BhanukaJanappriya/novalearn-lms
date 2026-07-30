using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.UpdateLesson;

public sealed class UpdateLessonCommandHandler(
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateLessonCommand, Result<LessonDto>>
{
    public async Task<Result<LessonDto>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        Lesson? lesson = await content.GetLessonByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
        {
            return Result.Failure<LessonDto>(ContentErrors.LessonNotFound);
        }

        if (ContentAuthority.CheckCanManage(lesson.Module?.Course, currentUser) is Error error)
        {
            return Result.Failure<LessonDto>(error);
        }

        lesson.Update(
            request.Title,
            request.Type,
            request.ContentUrl,
            request.TextContent,
            request.DurationMinutes,
            request.IsPreview);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return LessonDto.FromEntity(lesson);
    }
}
