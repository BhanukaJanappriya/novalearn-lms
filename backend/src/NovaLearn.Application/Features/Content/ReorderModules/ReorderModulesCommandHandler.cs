using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.ReorderModules;

public sealed class ReorderModulesCommandHandler(
    ICourseRepository courses,
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<ReorderModulesCommand, Result>
{
    public async Task<Result> Handle(ReorderModulesCommand request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);

        if (ContentAuthority.CheckCanManage(course, currentUser) is Error error)
        {
            return Result.Failure(error);
        }

        IReadOnlyList<CourseModule> modules =
            await content.GetModulesForCourseAsync(request.CourseId, cancellationToken);

        Dictionary<Guid, CourseModule> byId = modules.ToDictionary(m => m.Id);

        // Every supplied id must belong to this course, and the list must cover all of them,
        // so a partial or foreign order can never silently reshuffle the course.
        if (request.ModuleIds.Count != byId.Count || request.ModuleIds.Any(id => !byId.ContainsKey(id)))
        {
            return Result.Failure(ContentErrors.InvalidOrder);
        }

        for (int position = 0; position < request.ModuleIds.Count; position++)
        {
            byId[request.ModuleIds[position]].MoveTo(position);
        }

        // A single SaveChanges keeps the whole reshuffle in one transaction.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
