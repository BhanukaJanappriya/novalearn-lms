namespace NovaLearn.Application.Features.Authentication.Common;

/// <summary>Minimal user projection returned to clients alongside auth tokens.</summary>
public sealed record UserSummary(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);
