using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Content.Common;
using NovaLearn.Application.Features.Content.UpdateModule;
using NovaLearn.Domain.Content;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Content;

public sealed class UpdateModuleCommandHandlerTests
{
    private readonly ICourseContentRepository _content = Substitute.For<ICourseContentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly UpdateModuleCommandHandler _sut;

    public UpdateModuleCommandHandlerTests() =>
        _sut = new UpdateModuleCommandHandler(_content, _unitOfWork, _currentUser);

    private CourseModule ArrangeModuleOwnedBy(Guid lecturerId)
    {
        Course course = ContentTestData.NewCourse(lecturerId);
        CourseModule module = CourseModule.Create(course.Id, "Getting started", null, 0).WithCourse(course);

        _content.GetModuleByIdAsync(module.Id, Arg.Any<CancellationToken>()).Returns(module);
        return module;
    }

    [Fact]
    public async Task A_lecturer_cannot_edit_another_lecturers_module()
    {
        CourseModule module = ArrangeModuleOwnedBy(Guid.NewGuid());
        _currentUser.UserId.Returns(Guid.NewGuid()); // a different lecturer

        Result<ModuleDto> result = await _sut.Handle(
            new UpdateModuleCommand(module.Id, "Hijacked", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.NotOwner);
        module.Title.Should().Be("Getting started");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_owning_lecturer_can_edit_their_own_module()
    {
        var lecturerId = Guid.NewGuid();
        CourseModule module = ArrangeModuleOwnedBy(lecturerId);
        _currentUser.UserId.Returns(lecturerId);

        Result<ModuleDto> result = await _sut.Handle(
            new UpdateModuleCommand(module.Id, "Orientation", " Warm up. "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Orientation");
        result.Value.Description.Should().Be("Warm up.");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_administrator_can_edit_any_lecturers_module()
    {
        CourseModule module = ArrangeModuleOwnedBy(Guid.NewGuid());
        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsInRole(Roles.Administrator).Returns(true);

        Result<ModuleDto> result = await _sut.Handle(
            new UpdateModuleCommand(module.Id, "Orientation", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Orientation");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_module_is_reported_as_not_found()
    {
        _currentUser.UserId.Returns(Guid.NewGuid());
        _content.GetModuleByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CourseModule?)null);

        Result<ModuleDto> result = await _sut.Handle(
            new UpdateModuleCommand(Guid.NewGuid(), "Orientation", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ContentErrors.ModuleNotFound);
    }
}
