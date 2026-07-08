namespace NovaLearn.Shared.Results;

/// <summary>
/// Classifies an <see cref="Error"/> so the presentation layer can map it to the
/// correct transport concern (e.g. HTTP status code) without leaking domain details.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}

/// <summary>
/// An immutable, structured description of a failure. Errors carry a stable machine-readable
/// <see cref="Code"/> (for clients/i18n) alongside a human-readable <see cref="Description"/>.
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    /// <summary>Represents the absence of an error. Never surfaced on a failed result.</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
}
