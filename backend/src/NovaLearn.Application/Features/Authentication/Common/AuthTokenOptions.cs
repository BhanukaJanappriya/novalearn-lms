namespace NovaLearn.Application.Features.Authentication.Common;

/// <summary>Token lifetime settings used by the Application layer. Bound from the "Jwt" section.</summary>
public sealed class AuthTokenOptions
{
    public const string SectionName = "Jwt";

    public int RefreshTokenLifetimeDays { get; set; } = 7;
}
