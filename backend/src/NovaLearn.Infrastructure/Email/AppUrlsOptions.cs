namespace NovaLearn.Infrastructure.Email;

/// <summary>Public-facing URLs used to build links in transactional emails. Bound from "App".</summary>
public sealed class AppUrlsOptions
{
    public const string SectionName = "App";

    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}
