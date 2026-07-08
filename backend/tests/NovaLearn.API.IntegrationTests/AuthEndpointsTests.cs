using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NovaLearn.Domain.Identity;
using Xunit;

namespace NovaLearn.API.IntegrationTests;

public sealed class AuthEndpointsTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string Password = "Str0ng!Pass";

    [Fact]
    public async Task Register_confirm_then_login_returns_tokens()
    {
        HttpClient client = factory.CreateClient();
        string email = $"user-{Guid.NewGuid():N}@novalearn.local";

        // 1. Register
        HttpResponseMessage register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { firstName = "Ada", lastName = "Lovelace", email, password = Password });
        register.StatusCode.Should().Be(HttpStatusCode.OK);

        RegisterResponseDto? registered = await register.Content.ReadFromJsonAsync<RegisterResponseDto>();
        registered!.UserId.Should().NotBeEmpty();

        // 2. Login before verification is forbidden
        HttpResponseMessage early = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = Password });
        early.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // 3. Confirm the email (in production the token arrives by email)
        await ConfirmEmailAsync(email);

        // 4. Login succeeds and returns tokens
        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = Password });
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        AuthDto? auth = await login.Content.ReadFromJsonAsync<AuthDto>();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        auth.User.Roles.Should().Contain("Student");
    }

    [Fact]
    public async Task Duplicate_registration_returns_conflict()
    {
        HttpClient client = factory.CreateClient();
        string email = $"dupe-{Guid.NewGuid():N}@novalearn.local";
        object payload = new { firstName = "Grace", lastName = "Hopper", email, password = Password };

        (await client.PostAsJsonAsync("/api/v1/auth/register", payload)).StatusCode
            .Should().Be(HttpStatusCode.OK);
        (await client.PostAsJsonAsync("/api/v1/auth/register", payload)).StatusCode
            .Should().Be(HttpStatusCode.Conflict);
    }

    private async Task ConfirmEmailAsync(string email)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser user = (await userManager.FindByEmailAsync(email))!;
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
        result.Succeeded.Should().BeTrue();
    }

    private sealed record RegisterResponseDto(Guid UserId, string Email, bool RequiresEmailVerification);

    private sealed record AuthDto(string AccessToken, string RefreshToken, UserDto User);

    private sealed record UserDto(Guid Id, string Email, string FullName, string[] Roles);
}
