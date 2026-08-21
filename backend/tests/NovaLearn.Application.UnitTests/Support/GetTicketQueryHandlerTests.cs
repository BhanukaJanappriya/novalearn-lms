using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Application.Features.Support.GetTicket;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Support;

public sealed class GetTicketQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);

    private readonly ISupportTicketRepository _tickets = Substitute.For<ISupportTicketRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _submitterId = Guid.NewGuid();
    private readonly Guid _staffId = Guid.NewGuid();
    private readonly GetTicketQueryHandler _sut;

    public GetTicketQueryHandlerTests()
    {
        _sut = new GetTicketQueryHandler(_tickets, _currentUser);
    }

    private void SignedInAs(Guid userId, params string[] roles)
    {
        _currentUser.UserId.Returns(userId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private SupportTicket TicketWithInternalNote()
    {
        SupportTicket ticket = SupportTicket.Create(
            _submitterId, "Billing question", TicketCategory.Billing, TicketPriority.Low, "Help.", Now);
        ticket.Reply(_staffId, "Refund approved internally, do not tell the customer yet.", isInternalNote: true, Now);
        ticket.Reply(_staffId, "We're looking into your refund.", isInternalNote: false, Now);

        _tickets.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        return ticket;
    }

    private Task<Result<TicketDetailDto>> Act(Guid id) =>
        _sut.Handle(new GetTicketQuery(id), CancellationToken.None);

    [Fact]
    public async Task The_submitter_never_sees_an_internal_note()
    {
        SupportTicket ticket = TicketWithInternalNote();
        SignedInAs(_submitterId, Roles.Student);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Messages.Should().HaveCount(2, "the original report and the one non-internal reply");
        result.Value.Messages.Should().NotContain(m => m.IsInternalNote);
    }

    [Fact]
    public async Task Staff_see_the_internal_note_too()
    {
        SupportTicket ticket = TicketWithInternalNote();
        SignedInAs(Guid.NewGuid(), Roles.Administrator);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.Value.Messages.Should().HaveCount(3);
        result.Value.Messages.Should().Contain(m => m.IsInternalNote);
    }

    [Fact]
    public async Task A_stranger_cannot_view_someone_elses_ticket()
    {
        SupportTicket ticket = TicketWithInternalNote();
        SignedInAs(Guid.NewGuid(), Roles.Student);

        Result<TicketDetailDto> result = await Act(ticket.Id);

        result.Error.Should().Be(SupportErrors.NotOwnerOrStaff);
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
}
