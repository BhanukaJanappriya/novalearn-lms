using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Support.AssignTicket;
using NovaLearn.Application.Features.Support.ChangeTicketPriority;
using NovaLearn.Application.Features.Support.ChangeTicketStatus;
using NovaLearn.Application.Features.Support.Common;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Support;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Support;

public sealed class StaffTicketCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);

    private readonly ISupportTicketRepository _tickets = Substitute.For<ISupportTicketRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Guid _submitterId = Guid.NewGuid();

    public StaffTicketCommandHandlerTests() => _clock.UtcNow.Returns(Now);

    private void SignedInAs(params string[] roles) =>
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));

    private SupportTicket Ticket()
    {
        SupportTicket ticket = SupportTicket.Create(
            _submitterId, "Cannot log in", TicketCategory.Account, TicketPriority.Normal, "Help.", Now);
        _tickets.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        return ticket;
    }

    [Fact]
    public async Task A_student_cannot_change_a_tickets_status()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Roles.Student);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var sut = new ChangeTicketStatusCommandHandler(_tickets, _currentUser, _clock, _unitOfWork);

        Result<TicketDetailDto> result =
            await sut.Handle(new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Resolved), CancellationToken.None);

        result.Error.Should().Be(SupportErrors.StaffOnly);
        ticket.Status.Should().Be(TicketStatus.Open);
    }

    [Fact]
    public async Task Staff_can_resolve_a_ticket()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Roles.Administrator);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var sut = new ChangeTicketStatusCommandHandler(_tickets, _currentUser, _clock, _unitOfWork);

        Result<TicketDetailDto> result =
            await sut.Handle(new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Resolved), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TicketStatus.Resolved);
    }

    [Fact]
    public async Task A_lecturer_cannot_assign_a_ticket()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Roles.Lecturer);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var sut = new AssignTicketCommandHandler(_tickets, _currentUser, _unitOfWork);

        Result<TicketDetailDto> result =
            await sut.Handle(new AssignTicketCommand(ticket.Id, Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(SupportErrors.StaffOnly);
    }

    [Fact]
    public async Task Staff_can_claim_a_ticket_and_later_return_it_to_the_queue()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Roles.SuperAdministrator);
        Guid staffId = Guid.NewGuid();
        _currentUser.UserId.Returns(staffId);
        var sut = new AssignTicketCommandHandler(_tickets, _currentUser, _unitOfWork);

        Result<TicketDetailDto> claimed =
            await sut.Handle(new AssignTicketCommand(ticket.Id, staffId), CancellationToken.None);
        claimed.Value.AssignedToId.Should().Be(staffId);

        Result<TicketDetailDto> unassigned =
            await sut.Handle(new AssignTicketCommand(ticket.Id, null), CancellationToken.None);
        unassigned.Value.AssignedToId.Should().BeNull();
    }

    [Fact]
    public async Task A_student_cannot_reprioritise_a_ticket()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Roles.Student);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var sut = new ChangeTicketPriorityCommandHandler(_tickets, _currentUser, _unitOfWork);

        Result<TicketDetailDto> result =
            await sut.Handle(new ChangeTicketPriorityCommand(ticket.Id, TicketPriority.Urgent), CancellationToken.None);

        result.Error.Should().Be(SupportErrors.StaffOnly);
    }

    [Fact]
    public async Task Staff_can_escalate_a_tickets_priority()
    {
        SupportTicket ticket = Ticket();
        SignedInAs(Roles.Administrator);
        _currentUser.UserId.Returns(Guid.NewGuid());
        var sut = new ChangeTicketPriorityCommandHandler(_tickets, _currentUser, _unitOfWork);

        Result<TicketDetailDto> result =
            await sut.Handle(new ChangeTicketPriorityCommand(ticket.Id, TicketPriority.Urgent), CancellationToken.None);

        result.Value.Priority.Should().Be(TicketPriority.Urgent);
    }
}
