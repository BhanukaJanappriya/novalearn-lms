namespace NovaLearn.Domain.Support;

/// <summary>How urgently a ticket needs attention. Set by the submitter, adjustable by staff.</summary>
public enum TicketPriority
{
    Low,
    Normal,
    High,
    Urgent
}
