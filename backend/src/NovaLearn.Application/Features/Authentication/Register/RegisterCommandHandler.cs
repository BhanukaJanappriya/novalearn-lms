using MediatR;
using Microsoft.Extensions.Logging;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.Register;

/// <summary>
/// Creates the account via the identity port, then dispatches a verification email. The email is
/// sent from the handler (not a post-commit domain event) because the identity user is persisted
/// through <c>UserManager</c>, which sits outside our aggregate's SaveChanges pipeline.
/// </summary>
public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> creation = await identityService.CreateUserAsync(
            request.Email.Trim(),
            request.Password,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            cancellationToken);

        if (creation.IsFailure)
        {
            return Result.Failure<RegisterResponse>(creation.Error);
        }

        AuthenticatedUser user = creation.Value;

        string verificationToken = await identityService.GenerateEmailConfirmationTokenAsync(user.Id, cancellationToken);
        await emailSender.SendEmailVerificationAsync(
            user.Email, user.FullName, user.Id, verificationToken, cancellationToken);

        logger.LogInformation("Registered new user {UserId} ({Email})", user.Id, user.Email);

        return new RegisterResponse(user.Id, user.Email);
    }
}
