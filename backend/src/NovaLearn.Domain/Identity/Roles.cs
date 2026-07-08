namespace NovaLearn.Domain.Identity;

/// <summary>
/// Canonical role names. Used for seeding, <c>[Authorize(Roles = ...)]</c> attributes,
/// and policy configuration. Keep these in sync with the seeded roles in Persistence.
/// </summary>
public static class Roles
{
    public const string SuperAdministrator = "SuperAdministrator";
    public const string Administrator = "Administrator";
    public const string Lecturer = "Lecturer";
    public const string TeachingAssistant = "TeachingAssistant";
    public const string Student = "Student";
    public const string Guest = "Guest";

    /// <summary>The role assigned to a self-registered account by default.</summary>
    public const string Default = Student;

    public static readonly IReadOnlyList<string> All =
    [
        SuperAdministrator,
        Administrator,
        Lecturer,
        TeachingAssistant,
        Student,
        Guest
    ];
}
