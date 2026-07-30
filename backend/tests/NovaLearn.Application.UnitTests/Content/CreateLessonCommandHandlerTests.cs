using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Application.Features.Content.CreateLesson;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Content;

public sealed class CreateLessonCommandHandlerTests
{
    private readonly ICourseContentRepository _content = Substitute.For<ICourseContentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _lecturerId = Guid.NewGuid();
    private readonly CreateLessonCommandHandler _sut;

    public CreateLessonCommandHandlerTests() =>
        _sut = new CreateLessonCommandHandler(_content, _unitOfWork, _currentUser);

    private CourseModule ArrangeModule()
    {
        Course course = ContentTestData.NewCourse(_lecturerId);
        CourseModule module = CourseModule.Create(course.Id, "Getting started", null, 0).WithCourse(course);

        _content.GetModuleByIdAsync(module.Id, Arg.Any<CancellationToken>()).Returns(module);
        _content.NextLessonSortOrderAsync(module.Id, Arg.Any<CancellationToken>()).Returns(3);

        return module;
    }

    private static CreateLessonCommand CommandFor(Guid moduleId) =>
        new(moduleId, "Worked example", LessonType.Text, null, "Some body copy.", 12, false);

    /// <summary>
    /// Regression guard. BaseEntity assigns the key client-side, so a lesson reached only through
    /// the module's navigation collection is tracked as Modified and saves as a no-op UPDATE that
    /// throws DbUpdateConcurrencyException. The handler must state the insert explicitly.
    /// </summary>
    [Fact]
    public async Task The_new_lesson_is_explicitly_tracked_as_an_insert()
    {
        CourseModule module = ArrangeModule();
        _currentUser.UserId.Returns(_lecturerId);

        Result<LessonDto> result = await _sut.Handle(CommandFor(module.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _content.Received(1).AddLessonAsync(
            Arg.Is<Lesson>(l => l.Title == "Worked example"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_lesson_lands_at_the_end_of_the_module()
    {
        CourseModule module = ArrangeModule();
        _currentUser.UserId.Returns(_lecturerId);

        Result<LessonDto> result = await _sut.Handle(CommandFor(module.Id), CancellationToken.None);

        result.Value.SortOrder.Should().Be(3);
    }

    [Fact]
    public async Task A_lecturer_who_does_not_own_the_course_cannot_add_a_lesson()
    {
        CourseModule module = ArrangeModule();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Arg.Any<string>()).Returns(false);

        Result<LessonDto> result = await _sut.Handle(CommandFor(module.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _content.DidNotReceive().AddLessonAsync(Arg.Any<Lesson>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_administrator_may_add_a_lesson_to_any_course()
    {
        CourseModule module = ArrangeModule();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Roles.Administrator).Returns(true);

        Result<LessonDto> result = await _sut.Handle(CommandFor(module.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task A_missing_module_is_reported_as_not_found()
    {
        _content.GetModuleByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CourseModule?)null);
        _currentUser.UserId.Returns(_lecturerId);

        Result<LessonDto> result = await _sut.Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.ModuleNotFound);
    }
}
