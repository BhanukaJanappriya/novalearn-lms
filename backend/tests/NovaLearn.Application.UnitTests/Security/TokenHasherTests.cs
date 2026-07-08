using FluentAssertions;
using NovaLearn.Shared.Security;
using Xunit;

namespace NovaLearn.Application.UnitTests.Security;

public sealed class TokenHasherTests
{
    [Fact]
    public void Hash_is_deterministic()
    {
        const string token = "a-random-refresh-token-value";
        TokenHasher.Hash(token).Should().Be(TokenHasher.Hash(token));
    }

    [Fact]
    public void Verify_returns_true_for_matching_token()
    {
        const string token = "another-token";
        string hash = TokenHasher.Hash(token);
        TokenHasher.Verify(token, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_returns_false_for_different_token()
    {
        string hash = TokenHasher.Hash("token-one");
        TokenHasher.Verify("token-two", hash).Should().BeFalse();
    }
}
