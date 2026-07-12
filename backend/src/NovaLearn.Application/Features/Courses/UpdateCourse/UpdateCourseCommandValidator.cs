using FluentValidation;

namespace NovaLearn.Application.Features.Courses.UpdateCourse;

public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Za-z0-9- ]+$")
            .WithMessage("Code may contain only letters, numbers, spaces and hyphens.");

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Level)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(512);
    }
}
