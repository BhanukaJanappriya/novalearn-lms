using System.Reflection;
using FluentAssertions;
using FluentValidation.Results;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Directory;
using NovaLearn.Application.Features.Profile.Common;
using NovaLearn.Application.Features.Profile.UpdateAvatar;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Profile;

/// <summary>
/// Two promises are under test here. A person may only ever change their own picture, and the
/// people directory must not carry account-security state or an individual academic record.
/// </summary>
public sealed class ProfileOwnershipTests
{
    private readonly IProfileService _profiles = Substitute.For<IProfileService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly UpdateMyAvatarCommandHandler _sut;
    private readonly UpdateMyAvatarCommandValidator _validator = new();

    private static readonly Guid Me = Guid.NewGuid();

    public ProfileOwnershipTests()
    {
        _sut = new UpdateMyAvatarCommandHandler(_profiles, _currentUser);

        _profiles.SetAvatarAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _profiles.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new MyProfileDto(Me, "Amara Silva", "Amara", "Silva", "a@x.dev", null, ["Student"],
                DateTimeOffset.UtcNow));
    }

    // --- Ownership ----------------------------------------------------------------------

    /// <summary>
    /// The command carries no user id at all, so there is nothing for a caller to point at
    /// somebody else. This pins that structurally rather than trusting a check.
    /// </summary>
    [Fact]
    public void The_command_exposes_no_user_id_to_target_someone_else()
    {
        string[] properties = typeof(UpdateMyAvatarCommand)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        properties.Should().BeEquivalentTo(["AvatarUrl"]);
    }

    [Fact]
    public async Task The_avatar_is_written_against_the_signed_in_person()
    {
        _currentUser.UserId.Returns(Me);

        Result<MyProfileDto> result = await _sut.Handle(
            new UpdateMyAvatarCommand("https://cdn.example.com/me.png"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _profiles.Received(1).SetAvatarAsync(
            Me, "https://cdn.example.com/me.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unauthenticated_caller_changes_nothing()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<MyProfileDto> result = await _sut.Handle(
            new UpdateMyAvatarCommand("https://cdn.example.com/me.png"), CancellationToken.None);

        result.Error.Should().Be(ProfileErrors.Unauthenticated);
        await _profiles.DidNotReceive().SetAvatarAsync(
            Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Clearing_the_picture_is_allowed()
    {
        _currentUser.UserId.Returns(Me);

        Result<MyProfileDto> result = await _sut.Handle(
            new UpdateMyAvatarCommand(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _profiles.Received(1).SetAvatarAsync(Me, null, Arg.Any<CancellationToken>());
    }

    // --- The url is rendered into an img src, so the scheme matters ---------------------

    [Theory]
    [InlineData("https://cdn.example.com/me.png")]
    [InlineData("http://cdn.example.com/me.png")]
    [InlineData(null)]
    [InlineData("")]
    public void Safe_picture_links_are_accepted(string? url)
    {
        _validator.Validate(new UpdateMyAvatarCommand(url)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("javascript:alert(document.cookie)")]
    [InlineData("JavaScript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("file:///etc/passwd")]
    [InlineData("/relative/path.png")]
    [InlineData("not a url at all")]
    public void Dangerous_or_relative_picture_links_are_rejected(string url)
    {
        ValidationResult result = _validator.Validate(new UpdateMyAvatarCommand(url));

        result.IsValid.Should().BeFalse($"'{url}' must never reach an img src");
    }

    // --- The directory must stay non-sensitive -------------------------------------------

    /// <summary>
    /// The directory is a "who is here" view. Account-security state belongs to the account
    /// administration screen, where the authority checks live, and copying a field across from
    /// the admin row would be an easy mistake to make.
    /// </summary>
    [Fact]
    public void The_directory_row_carries_no_account_security_state()
    {
        string[] properties = typeof(DirectoryEntry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        properties.Should().NotContain("IsLockedOut");
        properties.Should().NotContain("AccessFailedCount");
        properties.Should().NotContain("EmailConfirmed");
        properties.Should().NotContain("PasswordHash");
        properties.Should().NotContain("SecurityStamp");
    }

    [Fact]
    public void The_directory_row_carries_no_individual_academic_record()
    {
        string[] properties = typeof(DirectoryLearnerStats)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        // Aggregates only: how many and how far, never which grade on which piece of work.
        properties.Should().BeEquivalentTo(
            ["EnrolledCourses", "CompletedCourses", "AverageProgressPercent"]);
    }

    [Fact]
    public void The_directory_dto_mirrors_the_row_and_adds_nothing()
    {
        string[] dto = typeof(DirectoryEntryDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        dto.Should().NotContain("IsLockedOut");
        dto.Should().NotContain("AccessFailedCount");
        dto.Should().NotContain("PasswordHash");
    }
}
