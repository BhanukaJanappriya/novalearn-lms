using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.ReorderLessons;

public sealed class ReorderLessonsCommandHandler(
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<ReorderLessonsCommand, Result>
{
    public async Task<Result> Handle(ReorderLessonsCommand request, CancellationToken cancellationToken)
    {
        CourseModule? module = await content.GetModuleByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Result.Failure(ContentErrors.ModuleNotFound);
        }

        if (ContentAuthority.CheckCanManage(module.Course, currentUser) is Error error)
        {
            return Result.Failure(error);
        }

        Dictionary<Guid, Lesson> byId = module.Lessons.ToDictionary(l => l.Id);

        // Every supplied id must belong to this module, and the list must cover all of them,
        // so a partial or foreign order can never silently reshuffle the module.
        if (request.LessonIds.Count != byId.Count || request.LessonIds.Any(id => !byId.ContainsKey(id)))
        {
            return Result.Failure(ContentErrors.InvalidOrder);
        }

        for (int position = 0; position < request.LessonIds.Count; position++)
        {
            byId[request.LessonIds[position]].MoveTo(position);
        }

        // A single SaveChanges keeps the whole reshuffle in one transaction.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
