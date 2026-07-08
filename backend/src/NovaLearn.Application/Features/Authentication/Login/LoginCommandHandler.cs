using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Authentication.Common;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IAuthTokenIssuer tokenIssuer)
    : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> validation =
            await identityService.ValidateCredentialsAsync(request.Email.Trim(), request.Password, cancellationToken);

        if (validation.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(validation.Error);
        }

        AuthenticatedUser user = validation.Value;

        AuthenticationResponse response = await tokenIssuer.IssueAsync(user, cancellationToken);
        await identityService.RecordSuccessfulLoginAsync(user.Id, cancellationToken);

        return response;
    }
}
