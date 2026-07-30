using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.DeleteLesson;

public sealed class DeleteLessonCommandHandler(
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteLessonCommand, Result>
{
    public async Task<Result> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        Lesson? lesson = await content.GetLessonByIdAsync(request.LessonId, cancellationToken);
        if (lesson is null)
        {
            return Result.Failure(ContentErrors.LessonNotFound);
        }

        if (ContentAuthority.CheckCanManage(lesson.Module?.Course, currentUser) is Error error)
        {
            return Result.Failure(error);
        }

        content.RemoveLesson(lesson);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
