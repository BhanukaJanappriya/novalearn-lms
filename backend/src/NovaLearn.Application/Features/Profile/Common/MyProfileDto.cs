namespace NovaLearn.Application.Features.Profile.Common;

/// <summary>
/// The caller's own profile. Returned only to the person it belongs to, so it may carry more
/// than the people directory does.
/// </summary>
public sealed record MyProfileDto(
    Guid Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string? AvatarUrl,
    IReadOnlyList<string> Roles,
    DateTimeOffset JoinedAtUtc);
