using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Authentication.VerifyEmail;

public sealed class VerifyEmailCommandHandler(IIdentityService identityService)
    : IRequestHandler<VerifyEmailCommand, Result>
{
    public Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken) =>
        identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
}
