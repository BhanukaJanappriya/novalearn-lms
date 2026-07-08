namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Sends transactional account emails. Implemented by an SMTP/provider adapter.</summary>
public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        string toEmail, string fullName, Guid userId, string verificationToken, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(
        string toEmail, string fullName, Guid userId, string resetToken, CancellationToken cancellationToken);
}
