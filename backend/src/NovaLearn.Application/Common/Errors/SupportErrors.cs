using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of support ticket failures.</summary>
public static class SupportErrors
{
    public static readonly Error Unauthenticated =
        Error.Unauthorized("support.unauthenticated", "You must be signed in to use support.");

    public static readonly Error NotFound =
        Error.NotFound("support.not_found", "The requested ticket was not found.");

    public static readonly Error NotOwnerOrStaff =
        Error.Forbidden("support.not_owner_or_staff", "You can only view your own tickets.");

    public static readonly Error StaffOnly =
        Error.Forbidden("support.staff_only", "Only an administrator can do that.");

    public static readonly Error InternalNoteNotAllowed =
        Error.Forbidden(
            "support.internal_note_not_allowed",
            "Only staff can leave an internal note.");
}
