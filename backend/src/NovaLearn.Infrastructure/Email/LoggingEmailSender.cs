using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.Infrastructure.Email;

/// <summary>
/// Development email sender that writes verification / reset links to the log instead of sending
/// real mail. Replace with an SMTP or provider-backed <see cref="IEmailSender"/> in production
/// (register the alternative in <c>DependencyInjection</c>).
/// </summary>
public sealed class LoggingEmailSender(
    ILogger<LoggingEmailSender> logger, IOptions<AppUrlsOptions> options) : IEmailSender
{
    private readonly AppUrlsOptions _urls = options.Value;

    public Task SendEmailVerificationAsync(
        string toEmail, string fullName, Guid userId, string verificationToken, CancellationToken cancellationToken)
    {
        string link = $"{_urls.FrontendBaseUrl}/verify-email?userId={userId}&token={WebUtility.UrlEncode(verificationToken)}";
        logger.LogInformation(
            "[DEV EMAIL] Verify email for {Email} ({Name}). Link: {Link}", toEmail, fullName, link);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string toEmail, string fullName, Guid userId, string resetToken, CancellationToken cancellationToken)
    {
        string link = $"{_urls.FrontendBaseUrl}/reset-password?userId={userId}&token={WebUtility.UrlEncode(resetToken)}";
        logger.LogInformation(
            "[DEV EMAIL] Password reset for {Email} ({Name}). Link: {Link}", toEmail, fullName, link);
        return Task.CompletedTask;
    }
}
