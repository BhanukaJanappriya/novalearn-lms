using Microsoft.Extensions.Options;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.Infrastructure.Storage;

/// <summary>
/// Exposes the configured size cap to the upload use case, so the rule is written down once and
/// the handler does not have to know about the storage options type.
/// </summary>
internal sealed class UploadLimits(IOptions<FileStorageOptions> options) : IUploadLimits
{
    public int MaxFileSizeMegabytes => options.Value.MaxFileSizeMegabytes;

    public long MaxFileSizeBytes => options.Value.MaxFileSizeBytes;
}
