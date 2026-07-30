using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Content.CreateModule;

public sealed class CreateModuleCommandHandler(
    ICourseRepository courses,
    ICourseContentRepository content,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateModuleCommand, Result<ModuleDto>>
{
    public async Task<Result<ModuleDto>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);

        if (ContentAuthority.CheckCanManage(course, currentUser) is Error error)
        {
            return Result.Failure<ModuleDto>(error);
        }

        int sortOrder = await content.NextModuleSortOrderAsync(request.CourseId, cancellationToken);

        CourseModule module = CourseModule.Create(
            request.CourseId, request.Title, request.Description, sortOrder);

        await content.AddModuleAsync(module, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ModuleDto.FromEntity(module);
    }
}
