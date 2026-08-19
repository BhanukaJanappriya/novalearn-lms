using Microsoft.Extensions.Options;
using NovaLearn.Application.Common.Interfaces;

namespace NovaLearn.Infrastructure.Email;

/// <summary>Implements <see cref="IFrontendUrls"/> over the same "App" configuration section the email sender uses.</summary>
internal sealed class FrontendUrls(IOptions<AppUrlsOptions> options) : IFrontendUrls
{
    public string Build(string path) =>
        $"{options.Value.FrontendBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}
