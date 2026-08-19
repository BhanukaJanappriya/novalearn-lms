namespace NovaLearn.Domain.Payments;

/// <summary>Where a payment sits in its lifecycle.</summary>
public enum PaymentStatus
{
    /// <summary>Checkout started; no money has moved yet.</summary>
    Pending,

    /// <summary>Confirmed paid. The enrolment this unlocked exists.</summary>
    Succeeded,

    /// <summary>The checkout session was abandoned or declined without paying.</summary>
    Failed,

    /// <summary>The checkout session's own time limit passed unpaid.</summary>
    Expired,

    /// <summary>Paid, then some of it given back.</summary>
    PartiallyRefunded,

    /// <summary>Paid, then all of it given back.</summary>
    Refunded
}
