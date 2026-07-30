using FluentValidation;

namespace NovaLearn.Application.Features.Enrollments.GetCourseRoster;

public sealed class GetCourseRosterQueryValidator : AbstractValidator<GetCourseRosterQuery>
{
    public GetCourseRosterQueryValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();
    }
}
