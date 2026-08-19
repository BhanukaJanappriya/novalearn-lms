namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// The frontend's own address, for use cases that need to build a link back into it (a payment
/// redirect, a verification link). Wraps the same configuration the transactional email sender
/// already uses, behind a port Application is allowed to depend on.
/// </summary>
public interface IFrontendUrls
{
    /// <summary>Joins a path onto the frontend's base address.</summary>
    string Build(string path);
}
