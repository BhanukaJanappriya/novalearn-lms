using FluentValidation;

namespace NovaLearn.Application.Features.Enrollments.EnrollInCourse;

public sealed class EnrollInCourseCommandValidator : AbstractValidator<EnrollInCourseCommand>
{
    public EnrollInCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();
    }
}
