using MediatR;
using NovaLearn.Application.Features.Admin.Users.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Users.CreateUser;

/// <summary>
/// An administrator adds an account directly (a lecturer, a student, a TA…). The account is
/// created with its email already confirmed and a single role, and can sign in immediately with
/// the password set here.
/// </summary>
public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Password)
    : IRequest<Result<AdminUserDto>>;
