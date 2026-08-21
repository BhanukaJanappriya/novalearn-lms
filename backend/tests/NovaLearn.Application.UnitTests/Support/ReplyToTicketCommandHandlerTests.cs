using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Application.Features.Support.ReplyToTicket;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Support;

public sealed class ReplyToTicketCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);

    private readonly ISupportTicketRepository _tickets = Substitute.For<ISupportTicketRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Guid _submitterId = Guid.NewGuid();
    private readonly ReplyToTicketCommandHandler _sut;

    public ReplyToTicketCommandHandlerTests()
    {
        _sut = new ReplyToTicketCommandHandler(_tickets, _currentUser, _clock, _unitOfWork);
        _clock.UtcNow.Returns(Now);
    }

    private void SignedInAs(Guid userId, params string[] roles)
    {
        _currentUser.UserId.Returns(userId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private SupportTicket Ticket()
    {
        SupportTicket ticket = SupportTicket.Create(
            _submitterId, "Cannot log in", TicketCategory.Account, TicketPriority.Normal, "Help.", Now);
        _tickets.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        return ticket;
    }

    private Task<Result<TicketDetailDto>> Act(Guid ticketId, string body = "A reply", bool internalNote = false) =>
        _sut.Handle(new ReplyToTicketCommand(ticketId, body, internalNote), CancellationToken.None);

    [Fact]
    public async Task The_submitter_can_reply_to_their_own_ticket()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(_submitterId, Roles.Student);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Staff_can_reply_to_a_ticket_they_do_not_own()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Guid.NewGuid(), Roles.Administrator);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_stranger_who_is_neither_the_submitter_nor_staff_is_refused()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Guid.NewGuid(), Roles.Student);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.Error.Should().Be(SupportErrors.NotOwnerOrStaff);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_submitter_cannot_mark_their_own_reply_as_an_internal_note()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(_submitterId, Roles.Student);

        Result<TicketDetailDto> result = await Act(ticket.Id, internalNote: true);

        result.Error.Should().Be(SupportErrors.InternalNoteNotAllowed);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Staff_can_leave_an_internal_note()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Guid.NewGuid(), Roles.Administrator);

        Result<TicketDetailDto> result = await Act(ticket.Id, internalNote: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Messages.Should().Contain(m => m.IsInternalNote);
    }

    [Fact]
    public async Task A_missing_ticket_is_reported()
    {
        Guid id = Guid.NewGuid();
        _tickets.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((SupportTicket?)null);
        SignedInAs(_submitterId, Roles.Student);

        Result<TicketDetailDto> result = await Act(id);

        result.Error.Should().Be(SupportErrors.NotFound);
    }

    [Fact]
    public async Task An_anonymous_caller_cannot_reply()
    {
        SupportTicket ticket = Ticket();
        _currentUser.UserId.Returns((Guid?)null);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.Error.Should().Be(SupportErrors.Unauthenticated);
    }
}
