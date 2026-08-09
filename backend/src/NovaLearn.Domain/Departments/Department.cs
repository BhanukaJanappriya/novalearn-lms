using NovaLearn.Domain.Common;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Domain.Departments;

/// <summary>
/// An academic department. Courses may belong to one, which is how the catalogue is organised
/// into faculties without forcing every course to pick one straight away.
///
/// Auditing and soft-delete come from <see cref="BaseEntity"/>. Constructed through
/// <see cref="Create"/> so the invariants (trimmed text, normalised code) hold from the start.
/// </summary>
public sealed class Department : BaseEntity
{
    private Department() { } // EF Core

    public string Name { get; private set; } = null!;

    /// <summary>Short human code, e.g. "PHYS". Unique and stored upper-cased.</summary>
    public string Code { get; private set; } = null!;

    public string? Description { get; private set; }

    /// <summary>The lecturer who heads the department, if one has been named.</summary>
    public Guid? HeadId { get; private set; }

    /// <summary>
    /// A retired department stays in the data so its courses keep their history, but it is not
    /// offered when assigning a new course.
    /// </summary>
    public bool IsActive { get; private set; }

    public ApplicationUser? Head { get; private set; }

    public static Department Create(
        string name, string code, string? description, Guid? headId, bool isActive = true) =>
        new()
        {
            Name = name.Trim(),
            Code = Normalise(code),
            Description = NormaliseOptional(description),
            HeadId = headId,
            IsActive = isActive,
        };

    /// <summary>Applies edited details, keeping the same invariants as <see cref="Create"/>.</summary>
    public void Update(string name, string code, string? description, Guid? headId, bool isActive)
    {
        Name = name.Trim();
        Code = Normalise(code);
        Description = NormaliseOptional(description);
        HeadId = headId;
        IsActive = isActive;
    }

    /// <summary>Names or clears the department head.</summary>
    public void AssignHead(Guid? headId) => HeadId = headId;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string Normalise(string code) => code.Trim().ToUpperInvariant();

    private static string? NormaliseOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
