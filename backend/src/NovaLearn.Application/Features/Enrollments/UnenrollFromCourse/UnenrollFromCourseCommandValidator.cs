using FluentValidation;

namespace NovaLearn.Application.Features.Enrollments.UnenrollFromCourse;

public sealed class UnenrollFromCourseCommandValidator : AbstractValidator<UnenrollFromCourseCommand>
{
    public UnenrollFromCourseCommandValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty();
    }
}
