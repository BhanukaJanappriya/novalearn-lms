using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for the people directory. Separate from <see cref="IUserDirectory"/>, which
/// backs account administration and carries the security state this deliberately omits.
/// </summary>
public interface IPeopleDirectory
{
    /// <summary>Everyone holding the Student role, with their learning totals.</summary>
    Task<IReadOnlyList<DirectoryEntry>> ListStudentsAsync(
        string? search, CancellationToken cancellationToken);

    /// <summary>
    /// Everyone who teaches: lecturers and teaching assistants, with what they are responsible
    /// for.
    /// </summary>
    Task<IReadOnlyList<DirectoryEntry>> ListTeachingStaffAsync(
        string? search, CancellationToken cancellationToken);
}
