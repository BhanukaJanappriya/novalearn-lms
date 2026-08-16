using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.Infrastructure.Storage;

/// <summary>Where uploaded files are written, and how large they may be.</summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Root directory for uploads, absolute or relative to the content root.</summary>
    public string Root { get; set; } = "App_Data/uploads";

    /// <summary>Largest single upload accepted, in megabytes.</summary>
    public int MaxFileSizeMegabytes { get; set; } = 200;

    public long MaxFileSizeBytes => MaxFileSizeMegabytes * 1024L * 1024L;
}

/// <summary>
/// Stores uploads on the local disk. Adequate for a single node and for development; the port
/// exists so this can become object storage without touching a use case.
///
/// Two rules make this safe to point at a directory:
///
/// Keys are generated here, never supplied. A key is a date prefix and a random name, so nothing a
/// user typed is ever used as a path, and two people uploading "notes.pdf" cannot collide.
///
/// Keys are validated again on the way back in. A row in the database is not treated as proof that
/// the string in it is a legal key, so a tampered or corrupted value cannot walk out of the root
/// directory.
/// </summary>
internal sealed class LocalFileStorage : IFileStorage
{
    /// <summary>The only shape a key may take: <c>2026/08/{32 hex}.{extension}</c>.</summary>
    private static readonly Regex KeyPattern = new(
        @"^\d{4}/\d{2}/[0-9a-f]{32}\.[a-z0-9]{1,8}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly FileStorageOptions _options;
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _root;

    public LocalFileStorage(
        IOptions<FileStorageOptions> options, ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _root = Path.GetFullPath(_options.Root);

        Directory.CreateDirectory(_root);
    }

    public async Task<StoredFile> SaveAsync(
        Stream content, string originalFileName, CancellationToken cancellationToken)
    {
        string extension = SafeExtension(originalFileName);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string key = $"{now:yyyy}/{now:MM}/{Guid.NewGuid():N}{extension}";

        string destination = ResolveWithinRoot(key)
            ?? throw new InvalidOperationException("Generated an invalid storage key.");

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        await using FileStream file = File.Create(destination);

        try
        {
            await content.CopyToAsync(file, cancellationToken);
        }
        catch
        {
            // Do not leave a half written file behind for a reader to find later.
            file.Close();
            TryDelete(destination);
            throw;
        }

        return new StoredFile(key, file.Length);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        string? path = ResolveWithinRoot(key);

        if (path is null || !File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        if (ResolveWithinRoot(key) is { } path)
        {
            TryDelete(path);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Turns a key into an absolute path, or null when it is not a key we would have issued.
    ///
    /// The pattern check alone would be enough, but the result is compared against the root as
    /// well: two independent reasons a path cannot escape are worth more than one.
    /// </summary>
    private string? ResolveWithinRoot(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !KeyPattern.IsMatch(key))
        {
            _logger.LogWarning("Rejected a storage key that does not match the expected shape.");
            return null;
        }

        string candidate = Path.GetFullPath(Path.Combine(_root, key));

        if (!candidate.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            _logger.LogWarning("Rejected a storage key that resolved outside the root directory.");
            return null;
        }

        return candidate;
    }

    /// <summary>
    /// The extension to store under, lowercased and stripped of anything unexpected. Callers have
    /// already checked the extension against the allowlist; this is belt and braces so a key can
    /// never contain a separator whatever it was handed.
    /// </summary>
    private static string SafeExtension(string originalFileName)
    {
        string extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (extension.Length is < 2 or > 9)
        {
            return ".bin";
        }

        return extension[1..].All(char.IsLetterOrDigit) ? extension : ".bin";
    }

    private void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Could not remove a stored file.");
        }
    }
}
