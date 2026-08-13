using NovaLearn.Application.Common.Models;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>
/// Read-side port for the cross course assessment list.
///
/// The scope is decided by the caller, not here: pass a lecturer id to see only that lecturer's
/// courses, or null for every course. Keeping the decision in the handler means the authority
/// rule lives next to the other assessment rules instead of being buried in a query.
/// </summary>
public interface IAssessmentOverview
{
    /// <summary>
    /// Every assignment and quiz on the courses in scope, newest deadline first.
    /// </summary>
    /// <param name="lecturerId">
    /// Restricts the result to courses owned by this lecturer. Null means no restriction, which
    /// only administrators are allowed to ask for.
    /// </param>
    Task<IReadOnlyList<AssessmentOverviewRow>> ListAsync(
        Guid? lecturerId, CancellationToken cancellationToken);
}
