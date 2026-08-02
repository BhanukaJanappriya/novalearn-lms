using FluentValidation;
using NovaLearn.Domain.Assessments;

namespace NovaLearn.Application.Features.Assessments.CreateAssignment;

public sealed class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Instructions).MaximumLength(20000);

        // The aggregate clamps too, but rejecting out-of-range input up front gives the client a
        // 400 with a field name instead of a silently adjusted value.
        RuleFor(x => x.MaxPoints).InclusiveBetween(1, Assignment.MaxPointsCeiling);

        RuleFor(x => x.Status).IsInEnum();
    }
}
