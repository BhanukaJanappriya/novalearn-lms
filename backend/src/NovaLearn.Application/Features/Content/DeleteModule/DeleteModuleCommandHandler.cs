using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.DeleteModule;

public sealed class DeleteModuleCommandHandler(
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteModuleCommand, Result>
{
    public async Task<Result> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
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

        // Removing the module also soft-deletes the lessons it carries.
        content.RemoveModule(module);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
