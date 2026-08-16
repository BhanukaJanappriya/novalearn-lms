namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// How large an upload may be.
///
/// A port rather than a constant because the cap is configuration, and the use case has to be able
/// to quote the number back in its error message without knowing where it came from.
/// </summary>
public interface IUploadLimits
{
    int MaxFileSizeMegabytes { get; }

    long MaxFileSizeBytes { get; }
}
