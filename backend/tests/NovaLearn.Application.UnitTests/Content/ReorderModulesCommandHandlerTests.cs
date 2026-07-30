using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.ReorderModules;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Content;

public sealed class ReorderModulesCommandHandlerTests
{
    private readonly ICourseRepository _courses = Substitute.For<ICourseRepository>();
    private readonly ICourseContentRepository _content = Substitute.For<ICourseContentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _lecturerId = Guid.NewGuid();
    private readonly Course _course;
    private readonly CourseModule _first;
    private readonly CourseModule _second;
    private readonly ReorderModulesCommandHandler _sut;

    public ReorderModulesCommandHandlerTests()
    {
        _course = ContentTestData.NewCourse(_lecturerId);
        _first = CourseModule.Create(_course.Id, "Getting started", null, 0);
        _second = CourseModule.Create(_course.Id, "Core concepts", null, 1);

        _currentUser.UserId.Returns(_lecturerId);
        _courses.GetByIdAsync(_course.Id, Arg.Any<CancellationToken>()).Returns(_course);
        _content.GetModulesForCourseAsync(_course.Id, Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<CourseModule>>([_first, _second]);

        _sut = new ReorderModulesCommandHandler(_courses, _content, _unitOfWork, _currentUser);
    }

    [Fact]
    public async Task A_module_id_from_another_course_is_rejected()
    {
        // Same count, but one id belongs to a different course.
        var foreignModuleId = Guid.NewGuid();

        Result result = await _sut.Handle(
            new ReorderModulesCommand(_course.Id, [foreignModuleId, _first.Id]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.InvalidOrder);
        _first.SortOrder.Should().Be(0);
        _second.SortOrder.Should().Be(1);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_partial_order_is_rejected()
    {
        Result result = await _sut.Handle(
            new ReorderModulesCommand(_course.Id, [_second.Id]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.InvalidOrder);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_complete_order_rewrites_every_position_once()
    {
        Result result = await _sut.Handle(
            new ReorderModulesCommand(_course.Id, [_second.Id, _first.Id]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _second.SortOrder.Should().Be(0);
        _first.SortOrder.Should().Be(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_lecturer_cannot_reorder_another_lecturers_course()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());

        Result result = await _sut.Handle(
            new ReorderModulesCommand(_course.Id, [_second.Id, _first.Id]), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.NotOwner);
    }
}
