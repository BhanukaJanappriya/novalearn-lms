using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Application.Features.Content.GetCourseContent;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Content;

public sealed class GetCourseContentQueryHandlerTests
{
    private readonly ICourseRepository _courses = Substitute.For<ICourseRepository>();
    private readonly ICourseContentRepository _content = Substitute.For<ICourseContentRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _lecturerId = Guid.NewGuid();
    private readonly GetCourseContentQueryHandler _sut;

    public GetCourseContentQueryHandlerTests() =>
        _sut = new GetCourseContentQueryHandler(_courses, _content, _currentUser);

    private Course ArrangeCourse(CourseStatus status)
    {
        Course course = ContentTestData.NewCourse(_lecturerId, status);
        CourseModule module = CourseModule.Create(course.Id, "Getting started", null, 0);
        module.AddLesson("Welcome", LessonType.Video, "https://videos.local/welcome.mp4", null, 6, 0, true);

        _courses.GetByIdAsync(course.Id, Arg.Any<CancellationToken>()).Returns(course);
        _content.GetModulesForCourseAsync(course.Id, Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<CourseModule>>([module]);

        return course;
    }

    [Fact]
    public async Task An_unrelated_student_cannot_read_a_draft_courses_content()
    {
        Course course = ArrangeCourse(CourseStatus.Draft);
        _currentUser.UserId.Returns(Guid.NewGuid()); // a student, not the owner and not an admin

        Result<CourseContentDto> result =
            await _sut.Handle(new GetCourseContentQuery(course.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.NotVisible);
    }

    [Fact]
    public async Task Any_signed_in_user_can_read_a_published_courses_content()
    {
        Course course = ArrangeCourse(CourseStatus.Published);
        _currentUser.UserId.Returns(Guid.NewGuid());

        Result<CourseContentDto> result =
            await _sut.Handle(new GetCourseContentQuery(course.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Modules.Should().ContainSingle();
        result.Value.Modules[0].Lessons.Should().ContainSingle()
            .Which.Type.Should().Be("Video");
    }

    [Fact]
    public async Task The_owning_lecturer_can_read_their_own_draft_content()
    {
        Course course = ArrangeCourse(CourseStatus.Draft);
        _currentUser.UserId.Returns(_lecturerId);

        Result<CourseContentDto> result =
            await _sut.Handle(new GetCourseContentQuery(course.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CourseStatus.Should().Be("Draft");
    }

    [Fact]
    public async Task An_administrator_can_read_any_draft_content()
    {
        Course course = ArrangeCourse(CourseStatus.Draft);
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Roles.SuperAdministrator).Returns(true);

        Result<CourseContentDto> result =
            await _sut.Handle(new GetCourseContentQuery(course.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_missing_course_is_reported_as_not_found()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _courses.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Course?)null);

        Result<CourseContentDto> result =
            await _sut.Handle(new GetCourseContentQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CourseErrors.NotFound);
    }
}
