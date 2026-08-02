using FluentValidation;
using NovaLearn.Domain.Assessments;

namespace NovaLearn.Application.Features.Assessments.UpdateAssignment;

public sealed class UpdateAssignmentCommandValidator : AbstractValidator<UpdateAssignmentCommand>
{
    public UpdateAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Instructions).MaximumLength(20000);

        RuleFor(x => x.MaxPoints).InclusiveBetween(1, Assignment.MaxPointsCeiling);

        RuleFor(x => x.Status).IsInEnum();
    }
}
