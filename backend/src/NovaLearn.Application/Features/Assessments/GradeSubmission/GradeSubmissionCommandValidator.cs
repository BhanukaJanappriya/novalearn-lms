using FluentValidation;
using NovaLearn.Domain.Assessments;

namespace NovaLearn.Application.Features.Assessments.GradeSubmission;

public sealed class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
{
    public GradeSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();

        // The ceiling for the specific assignment is enforced in the handler, which knows it;
        // this only rejects obvious nonsense before any work is done.
        RuleFor(x => x.PointsAwarded).InclusiveBetween(0, Assignment.MaxPointsCeiling);

        RuleFor(x => x.Feedback).MaximumLength(4000);
    }
}
