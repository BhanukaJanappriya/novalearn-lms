namespace NovaLearn.Persistence.Seeding;

/// <summary>Bootstrap seed data, bound from the "Seed" configuration section.</summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string SuperAdminEmail { get; set; } = "admin@novalearn.local";
    public string SuperAdminPassword { get; set; } = string.Empty;
    public string SuperAdminFirstName { get; set; } = "Nova";
    public string SuperAdminLastName { get; set; } = "Administrator";
}
