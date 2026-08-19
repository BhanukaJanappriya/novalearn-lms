using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Payments.Events;

/// <summary>
/// Raised the moment a payment is confirmed, before the enrolment it pays for necessarily exists
/// yet in the handler's view — carries everything a notification needs so it does not have to
/// load the course again.
/// </summary>
public sealed record PaymentSucceededDomainEvent(
    Guid PaymentId,
    Guid StudentId,
    Guid CourseId,
    string CourseTitle,
    decimal Amount,
    string Currency) : DomainEvent;
