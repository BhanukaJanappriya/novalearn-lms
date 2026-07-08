using MediatR;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.Login;

/// <summary>Authenticates a user with email + password and issues access/refresh tokens.</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthenticationResponse>>;
