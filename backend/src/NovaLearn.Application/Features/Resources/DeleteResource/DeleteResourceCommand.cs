using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Resources.DeleteResource;

/// <summary>Removes a post from the wall.</summary>
public sealed record DeleteResourceCommand(Guid ResourceId) : IRequest<Result>;

/// <summary>
/// Takes a post off the wall.
///
/// The row is soft deleted, in line with every other aggregate here, and the stored bytes are
/// left where they are. That is deliberate rather than an oversight: deleting the file would make
/// the soft delete a lie, since the row could be restored and would then point at nothing.
/// Reclaiming space belongs to a sweep that can tell which keys no live row refers to any more.
/// </summary>
public sealed class DeleteResourceCommandHandler(
    IResourceRepository resources, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteResourceCommand, Result>
{
    public async Task<Result> Handle(
        DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure(ResourceErrors.Unauthenticated);
        }

        Resource? resource = await resources.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
        {
            return Result.Failure(ResourceErrors.NotFound);
        }

        if (!ResourceAuthority.CanManage(resource, currentUser))
        {
            return Result.Failure(ResourceErrors.NotPoster);
        }

        resources.Remove(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
