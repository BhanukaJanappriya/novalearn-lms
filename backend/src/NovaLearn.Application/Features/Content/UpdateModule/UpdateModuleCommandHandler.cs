using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.UpdateModule;

public sealed class UpdateModuleCommandHandler(
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateModuleCommand, Result<ModuleDto>>
{
    public async Task<Result<ModuleDto>> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        CourseModule? module = await content.GetModuleByIdAsync(request.ModuleId, cancellationToken);
        if (module is null)
        {
            return Result.Failure<ModuleDto>(ContentErrors.ModuleNotFound);
        }

        if (ContentAuthority.CheckCanManage(module.Course, currentUser) is Error error)
        {
            return Result.Failure<ModuleDto>(error);
        }

        module.Update(request.Title, request.Description);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ModuleDto.FromEntity(module);
    }
}
