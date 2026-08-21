using FluentAssertions;
using NovaLearn.Domain.Support;
using NovaLearn.Domain.Support.Events;
using Xunit;

namespace NovaLearn.Application.UnitTests.Support;

public sealed class SupportTicketTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);
    private readonly Guid _submitterId = Guid.NewGuid();
    private readonly Guid _staffId = Guid.NewGuid();

    private SupportTicket NewTicket() =>
        SupportTicket.Create(
            _submitterId, "Cannot access my course", TicketCategory.Technical, TicketPriority.Normal,
            "The video player never loads.", Now);

    [Fact]
    public void Creating_a_ticket_seeds_the_thread_with_the_first_message()
    {
        SupportTicket ticket = NewTicket();

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.Messages.Should().ContainSingle();
        ticket.Messages.Single().AuthorId.Should().Be(_submitterId);
        ticket.Messages.Single().IsInternalNote.Should().BeFalse();
        ticket.MessageCount.Should().Be(1);
    }

    [Fact]
    public void A_staff_reply_to_an_open_ticket_moves_it_to_in_progress()
    {
        SupportTicket ticket = NewTicket();

        ticket.Reply(_staffId, "Looking into it now.", isInternalNote: false, Now);

        ticket.Status.Should().Be(TicketStatus.InProgress);
    }

    [Fact]
    public void An_internal_note_never_changes_status()
    {
        SupportTicket ticket = NewTicket();

        ticket.Reply(_staffId, "Known issue, escalating to platform team.", isInternalNote: true, Now);

        ticket.Status.Should().Be(TicketStatus.Open, "an internal note is staff talking to each other, not to the submitter");
    }

    [Theory]
    [InlineData(TicketStatus.Resolved)]
    [InlineData(TicketStatus.Closed)]
    public void The_submitter_replying_to_a_settled_ticket_reopens_it(TicketStatus settled)
    {
        SupportTicket ticket = NewTicket();
        ticket.ChangeStatus(settled, Now);

        ticket.Reply(_submitterId, "This is still happening.", isInternalNote: false, Now.AddHours(1));

        ticket.Status.Should().Be(TicketStatus.Open);
    }

    [Fact]
    public void Staff_replying_to_a_resolved_ticket_does_not_reopen_it()
    {
        SupportTicket ticket = NewTicket();
        ticket.ChangeStatus(TicketStatus.Resolved, Now);

        ticket.Reply(_staffId, "Following up on the fix.", isInternalNote: false, Now.AddHours(1));

        ticket.Status.Should().Be(TicketStatus.Resolved);
    }

    [Fact]
    public void The_submitter_cannot_leave_an_internal_note()
    {
        SupportTicket ticket = NewTicket();

        Action act = () => ticket.Reply(_submitterId, "Trying to sneak a note in.", isInternalNote: true, Now);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void A_non_internal_reply_raises_a_replied_event_addressed_to_the_other_side()
    {
        SupportTicket ticket = NewTicket();
        ticket.ClearDomainEvents();
        ticket.AssignTo(_staffId);

        ticket.Reply(_submitterId, "Any update?", isInternalNote: false, Now);

        ticket.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SupportTicketRepliedDomainEvent>()
            .Which.RecipientId.Should().Be(_staffId);
    }

    [Fact]
    public void A_submitter_reply_on_an_unclaimed_ticket_names_no_recipient()
    {
        SupportTicket ticket = NewTicket();
        ticket.ClearDomainEvents();

        ticket.Reply(_submitterId, "Any update?", isInternalNote: false, Now);

        var raised = (SupportTicketRepliedDomainEvent)ticket.DomainEvents.Single();
        raised.RecipientId.Should().BeNull("nobody has claimed the ticket yet");
    }

    [Fact]
    public void An_internal_note_raises_no_replied_event()
    {
        SupportTicket ticket = NewTicket();
        ticket.ClearDomainEvents();

        ticket.Reply(_staffId, "Internal context only.", isInternalNote: true, Now);

        ticket.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Changing_to_the_same_status_is_a_no_op_and_raises_nothing()
    {
        SupportTicket ticket = NewTicket();
        ticket.ClearDomainEvents();

        ticket.ChangeStatus(TicketStatus.Open, Now);

        ticket.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Resolving_stamps_the_time_and_closing_stamps_a_different_one()
    {
        SupportTicket ticket = NewTicket();

        ticket.ChangeStatus(TicketStatus.Resolved, Now);
        ticket.ResolvedAtUtc.Should().Be(Now);
        ticket.ClosedAtUtc.Should().BeNull();

        ticket.ChangeStatus(TicketStatus.Closed, Now.AddDays(1));
        ticket.ClosedAtUtc.Should().Be(Now.AddDays(1));

        // Only one of the two is ever set at a time — closing does not leave a stale resolved time.
        ticket.ResolvedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Assigning_and_unassigning_move_the_ticket_between_the_claimed_and_unclaimed_queues()
    {
        SupportTicket ticket = NewTicket();

        ticket.AssignTo(_staffId);
        ticket.AssignedToId.Should().Be(_staffId);

        ticket.Unassign();
        ticket.AssignedToId.Should().BeNull();
    }

    [Fact]
    public void Last_activity_tracks_the_most_recent_message_not_the_ticket_creation()
    {
        SupportTicket ticket = NewTicket();
        ticket.Reply(_staffId, "First response.", isInternalNote: false, Now.AddDays(2));

        ticket.LastActivityAtUtc.Should().Be(Now.AddDays(2));
    }
}
