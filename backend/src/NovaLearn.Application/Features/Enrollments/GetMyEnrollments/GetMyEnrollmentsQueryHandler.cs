using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Enrollments.GetMyEnrollments;

public sealed class GetMyEnrollmentsQueryHandler(
    IEnrollmentRepository enrollments,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyEnrollmentsQuery, Result<IReadOnlyList<EnrollmentDto>>>
{
    public async Task<Result<IReadOnlyList<EnrollmentDto>>> Handle(
        GetMyEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid studentId)
        {
            return Result.Failure<IReadOnlyList<EnrollmentDto>>(EnrollmentErrors.Unauthenticated);
        }

        IReadOnlyList<Enrollment> mine = await enrollments.ListForStudentAsync(studentId, cancellationToken);
        IReadOnlyList<EnrollmentDto> dtos = mine.Select(EnrollmentDto.FromEntity).ToList();

        return Result.Success(dtos);
    }
}
