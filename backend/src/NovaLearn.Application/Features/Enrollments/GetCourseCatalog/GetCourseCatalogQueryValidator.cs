using FluentValidation;

namespace NovaLearn.Application.Features.Enrollments.GetCourseCatalog;

public sealed class GetCourseCatalogQueryValidator : AbstractValidator<GetCourseCatalogQuery>
{
    public GetCourseCatalogQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.Level)
            .IsInEnum()
            .When(x => x.Level.HasValue);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 60);
    }
}
