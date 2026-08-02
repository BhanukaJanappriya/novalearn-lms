using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Assessments.Common;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Assessments.CreateAssignment;

public sealed class CreateAssignmentCommandHandler(
    ICourseRepository courses,
    IAssessmentRepository assessments,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTime)
    : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(
        CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        Course? course = await courses.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<AssignmentDto>(AssessmentErrors.CourseNotFound);
        }

        if (AssessmentAuthority.CheckCanManage(course, currentUser) is { } denied)
        {
            return Result.Failure<AssignmentDto>(denied);
        }

        Assignment assignment = Assignment.Create(
            request.CourseId,
            request.Title,
            request.Instructions,
            request.DueAtUtc,
            request.MaxPoints,
            request.AllowLateSubmissions,
            request.Status);

        await assessments.AddAssignmentAsync(assignment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AssignmentDto.FromEntity(assignment, dateTime.UtcNow);
    }
}
