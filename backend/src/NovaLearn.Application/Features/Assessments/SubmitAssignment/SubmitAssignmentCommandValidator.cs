using FluentValidation;

namespace NovaLearn.Application.Features.Assessments.SubmitAssignment;

public sealed class SubmitAssignmentCommandValidator : AbstractValidator<SubmitAssignmentCommand>
{
    public SubmitAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();

        // An attachment link alone is not a submission; there must be something to read.
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);

        RuleFor(x => x.AttachmentUrl).MaximumLength(1024);
    }
}
