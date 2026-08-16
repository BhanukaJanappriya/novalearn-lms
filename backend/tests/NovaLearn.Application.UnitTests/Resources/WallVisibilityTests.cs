using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Resources.Common;
using NovaLearn.Application.Features.Resources.GetWall;
using NovaLearn.Domain.Identity;
using NovaLearn.Domain.Resources;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Resources;

public sealed class WallVisibilityTests
{
    private readonly IResourceRepository _resources = Substitute.For<IResourceRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly GetWallQueryHandler _sut;

    public WallVisibilityTests()
    {
        _sut = new GetWallQueryHandler(_resources, _currentUser);

        _resources.ListWallAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<Guid?>(),
                Arg.Any<ResourceKind?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<Resource>>([]);

        _resources.VisibleCourseIdsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<Guid>>([_courseId]);
    }

    private void SignedInAs(params string[] roles)
    {
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private Task<Result<IReadOnlyList<ResourceDto>>> Act() =>
        _sut.Handle(new GetWallQuery(null, null, null), CancellationToken.None);

    [Fact]
    public async Task An_anonymous_caller_is_refused()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<IReadOnlyList<ResourceDto>> result = await Act();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResourceErrors.Unauthenticated);
    }

    [Fact]
    public async Task A_student_only_sees_posts_for_courses_they_are_on()
    {
        SignedInAs(Roles.Student);

        await Act();

        // The course set is worked out first and the query built against it, so a post from a
        // course they are not on never enters the result rather than being stripped afterwards.
        await _resources.Received(1).VisibleCourseIdsAsync(_callerId, Arg.Any<CancellationToken>());
        await _resources.Received(1).ListWallAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(_courseId)),
            false,
            Arg.Any<Guid?>(), Arg.Any<ResourceKind?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(Roles.Administrator)]
    [InlineData(Roles.SuperAdministrator)]
    public async Task An_administrator_sees_the_whole_wall_without_a_course_lookup(string role)
    {
        SignedInAs(role);

        await Act();

        await _resources.Received(1).ListWallAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            true,
            Arg.Any<Guid?>(), Arg.Any<ResourceKind?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

        await _resources.DidNotReceive().VisibleCourseIdsAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_lecturer_is_scoped_like_anyone_else_rather_than_seeing_everything()
    {
        SignedInAs(Roles.Lecturer);

        await Act();

        // Teaching a course is what puts it in the visible set; the role alone does not.
        await _resources.Received(1).ListWallAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            false,
            Arg.Any<Guid?>(), Arg.Any<ResourceKind?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Only_the_poster_and_administrators_are_told_they_can_manage_a_post()
    {
        Guid someoneElse = Guid.NewGuid();

        Resource mine = Resource.ForLink("Mine", null, "https://example.com/a", null, _callerId);
        Resource theirs = Resource.ForLink("Theirs", null, "https://example.com/b", null, someoneElse);

        _resources.ListWallAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<bool>(), Arg.Any<Guid?>(),
                Arg.Any<ResourceKind?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<Resource>>([mine, theirs]);

        SignedInAs(Roles.Lecturer);

        Result<IReadOnlyList<ResourceDto>> result = await Act();

        result.Value.Single(r => r.Title == "Mine").CanManage.Should().BeTrue();
        result.Value.Single(r => r.Title == "Theirs").CanManage.Should().BeFalse();
    }

    [Fact]
    public void A_youtube_post_carries_its_thumbnail_to_the_client()
    {
        Resource resource = Resource.ForLink(
            "Lecture recording", null, "https://youtu.be/dQw4w9WgXcQ", null, _callerId);

        ResourceDto dto = ResourceMapper.ToDto(resource, canManage: false);

        dto.Kind.Should().Be(ResourceKind.YouTube);
        dto.ThumbnailUrl.Should().Be("https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg");
        dto.EmbedUrl.Should().NotBeNull();
    }

    [Fact]
    public void An_upload_is_addressed_by_an_api_route_rather_than_a_storage_path()
    {
        Resource resource = Resource.ForUpload(
            "Week 1 notes", null, ResourceKind.Pdf,
            storedFileKey: "2026/08/0123456789abcdef0123456789abcdef.pdf",
            originalFileName: "notes.pdf",
            contentType: "application/pdf",
            sizeBytes: 2048,
            courseId: null,
            postedById: _callerId);

        ResourceDto dto = ResourceMapper.ToDto(resource, canManage: false);

        // Where the bytes actually live must never reach the browser, or the visibility check on
        // the download route could be walked around.
        dto.FileUrl.Should().Be($"/api/v1/resources/{resource.Id}/file");
        dto.Url.Should().BeNull();
        System.Text.Json.JsonSerializer.Serialize(dto)
            .Should().NotContain("0123456789abcdef");
    }
}
