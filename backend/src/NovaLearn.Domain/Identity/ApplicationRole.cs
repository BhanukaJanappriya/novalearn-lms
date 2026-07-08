using Microsoft.AspNetCore.Identity;

namespace NovaLearn.Domain.Identity;

/// <summary>
/// Application role. Extends Identity's role with a human-friendly description and a flag
/// marking system roles that must not be edited or deleted through the admin UI.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName, string description, bool isSystemRole = true)
        : base(roleName)
    {
        Description = description;
        IsSystemRole = isSystemRole;
    }

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }
}
