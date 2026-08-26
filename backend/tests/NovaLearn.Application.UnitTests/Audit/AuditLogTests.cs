using FluentAssertions;
using NovaLearn.Domain.Audit;
using Xunit;

namespace NovaLearn.Application.UnitTests.Audit;

public sealed class AuditLogTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
    private readonly Guid _actorId = Guid.NewGuid();

    [Fact]
    public void Creating_an_entry_stamps_everything_passed_in()
    {
        Guid entityId = Guid.NewGuid();

        AuditLog log = AuditLog.Create(
            _actorId, AuditCategory.Finance, "Refunded payment", "80 usd for Intro to Programming",
            "Payment", entityId, Now);

        log.ActorId.Should().Be(_actorId);
        log.Category.Should().Be(AuditCategory.Finance);
        log.Action.Should().Be("Refunded payment");
        log.Details.Should().Be("80 usd for Intro to Programming");
        log.EntityType.Should().Be("Payment");
        log.EntityId.Should().Be(entityId);
        log.CreatedAtUtc.Should().Be(Now);
    }

    [Fact]
    public void An_action_is_required()
    {
        Action act = () => AuditLog.Create(_actorId, AuditCategory.Settings, "  ", null, null, null, Now);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Blank_details_and_entity_type_become_null_rather_than_empty()
    {
        AuditLog log = AuditLog.Create(_actorId, AuditCategory.Courses, "Deleted course", "   ", "   ", null, Now);

        log.Details.Should().BeNull();
        log.EntityType.Should().BeNull();
    }

    [Fact]
    public void Action_and_details_are_trimmed()
    {
        AuditLog log = AuditLog.Create(
            _actorId, AuditCategory.UserManagement, "  Deactivated account  ", "  Jane Learner  ", null, null, Now);

        log.Action.Should().Be("Deactivated account");
        log.Details.Should().Be("Jane Learner");
    }
}
