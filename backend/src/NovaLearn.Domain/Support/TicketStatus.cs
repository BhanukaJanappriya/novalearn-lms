namespace NovaLearn.Domain.Support;

/// <summary>Where a support ticket sits in its lifecycle.</summary>
public enum TicketStatus
{
    /// <summary>Raised, nobody on staff has answered yet.</summary>
    Open,

    /// <summary>Staff have replied at least once and are working it.</summary>
    InProgress,

    /// <summary>Staff consider it answered. The submitter can still reopen it.</summary>
    Resolved,

    /// <summary>Settled. No further action expected from either side.</summary>
    Closed
}
