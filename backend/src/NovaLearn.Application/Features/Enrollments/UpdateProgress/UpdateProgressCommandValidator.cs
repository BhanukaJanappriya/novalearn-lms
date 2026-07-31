using FluentValidation;
using NovaLearn.Domain.Enrollments;

namespace NovaLearn.Application.Features.Enrollments.UpdateProgress;

public sealed class UpdateProgressCommandValidator : AbstractValidator<UpdateProgressCommand>
{
    public UpdateProgressCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty();

        // The aggregate clamps too, but rejecting out-of-range input up front gives the client a
        // 400 with a field name instead of a silently adjusted value.
        RuleFor(x => x.ProgressPercent)
            .InclusiveBetween(0, Enrollment.MaxProgressPercent);
    }
}
