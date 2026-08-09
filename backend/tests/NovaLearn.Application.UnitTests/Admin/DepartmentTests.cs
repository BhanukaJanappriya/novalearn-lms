using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Admin.Departments.Common;
using NovaLearn.Application.Features.Admin.Departments.DeleteDepartment;
using NovaLearn.Application.Features.Admin.Departments.SaveDepartment;
using NovaLearn.Domain.Departments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class DepartmentTests
{
    private readonly IDepartmentRepository _departments = Substitute.For<IDepartmentRepository>();
    private readonly IUserDirectory _users = Substitute.For<IUserDirectory>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly SaveDepartmentCommandHandler _save;
    private readonly DeleteDepartmentCommandHandler _delete;

    public DepartmentTests()
    {
        _save = new SaveDepartmentCommandHandler(_departments, _users, _unitOfWork);
        _delete = new DeleteDepartmentCommandHandler(_departments, _unitOfWork);

        // Nothing is in use and nothing has courses unless a test says so.
        _departments.CountCoursesAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
    }

    private void ExistingDepartment(Department department) =>
        _departments.GetByIdAsync(department.Id, Arg.Any<CancellationToken>()).Returns(department);

    private void UserWithRoles(Guid id, params string[] roles) =>
        _users.GetAsync(id, Arg.Any<CancellationToken>()).Returns(
            new AdminUserRow(
                id, "Nuwan", "Perera", "n@x.dev", null, true, true, false,
                DateTimeOffset.UtcNow, null, roles, 0, 0));

    // --- The aggregate ------------------------------------------------------------------

    [Fact]
    public void A_code_is_stored_upper_cased_and_trimmed()
    {
        Department department = Department.Create("  Physics  ", "  phys ", "  Waves.  ", null);

        department.Name.Should().Be("Physics");
        department.Code.Should().Be("PHYS");
        department.Description.Should().Be("Waves.");
        department.IsActive.Should().BeTrue();
    }

    [Fact]
    public void A_blank_description_becomes_null_rather_than_empty()
    {
        Department.Create("Physics", "PHYS", "   ", null).Description.Should().BeNull();
    }

    [Fact]
    public void Retiring_a_department_keeps_it_for_its_courses_history()
    {
        Department department = Department.Create("Physics", "PHYS", null, null);

        department.Deactivate();
        department.IsActive.Should().BeFalse();

        department.Activate();
        department.IsActive.Should().BeTrue();
    }

    [Fact]
    public void A_head_can_be_named_and_cleared()
    {
        Department department = Department.Create("Physics", "PHYS", null, null);
        Guid head = Guid.NewGuid();

        department.AssignHead(head);
        department.HeadId.Should().Be(head);

        department.AssignHead(null);
        department.HeadId.Should().BeNull();
    }

    // --- Saving -------------------------------------------------------------------------

    [Fact]
    public async Task A_duplicate_code_is_refused()
    {
        _departments.CodeExistsAsync("PHYS", null, Arg.Any<CancellationToken>()).Returns(true);

        Result<DepartmentDto> result = await _save.Handle(
            new SaveDepartmentCommand(null, "Physics", "PHYS", null, null, true), CancellationToken.None);

        result.Error.Should().Be(DepartmentErrors.CodeInUse);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Editing a department must not collide with its own code.</summary>
    [Fact]
    public async Task Renaming_a_department_keeps_its_own_code()
    {
        Department existing = Department.Create("Physics", "PHYS", null, null);
        ExistingDepartment(existing);
        _departments.CodeExistsAsync("PHYS", existing.Id, Arg.Any<CancellationToken>()).Returns(false);

        Result<DepartmentDto> result = await _save.Handle(
            new SaveDepartmentCommand(existing.Id, "Physics and Astronomy", "PHYS", null, null, true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.Name.Should().Be("Physics and Astronomy");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_student_cannot_be_made_a_department_head()
    {
        Guid student = Guid.NewGuid();
        UserWithRoles(student, Roles.Student);

        Result<DepartmentDto> result = await _save.Handle(
            new SaveDepartmentCommand(null, "Physics", "PHYS", null, student, true), CancellationToken.None);

        result.Error.Should().Be(DepartmentErrors.HeadNotALecturer);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_lecturer_can_be_made_a_department_head()
    {
        Guid lecturer = Guid.NewGuid();
        UserWithRoles(lecturer, Roles.Lecturer);

        Department created = Department.Create("Physics", "PHYS", null, lecturer);
        _departments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(created);

        Result<DepartmentDto> result = await _save.Handle(
            new SaveDepartmentCommand(null, "Physics", "PHYS", null, lecturer, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _departments.Received(1).AddAsync(Arg.Any<Department>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_head_who_does_not_exist_is_refused()
    {
        _users.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AdminUserRow?)null);

        Result<DepartmentDto> result = await _save.Handle(
            new SaveDepartmentCommand(null, "Physics", "PHYS", null, Guid.NewGuid(), true),
            CancellationToken.None);

        result.Error.Should().Be(DepartmentErrors.HeadNotALecturer);
    }

    [Fact]
    public async Task Editing_a_department_that_is_gone_is_reported_as_not_found()
    {
        _departments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Department?)null);

        Result<DepartmentDto> result = await _save.Handle(
            new SaveDepartmentCommand(Guid.NewGuid(), "Physics", "PHYS", null, null, true),
            CancellationToken.None);

        result.Error.Should().Be(DepartmentErrors.NotFound);
    }

    // --- Deleting -----------------------------------------------------------------------

    /// <summary>
    /// Deleting out from under live courses would quietly strip their department, so it is
    /// refused and the caller is pointed at retiring instead.
    /// </summary>
    [Fact]
    public async Task A_department_with_courses_cannot_be_deleted()
    {
        Department department = Department.Create("Physics", "PHYS", null, null);
        ExistingDepartment(department);
        _departments.CountCoursesAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [department.Id] = 3 });

        Result result = await _delete.Handle(
            new DeleteDepartmentCommand(department.Id), CancellationToken.None);

        result.Error.Should().Be(DepartmentErrors.HasCourses);
        _departments.DidNotReceive().Remove(Arg.Any<Department>());
    }

    [Fact]
    public async Task An_empty_department_can_be_deleted()
    {
        Department department = Department.Create("Physics", "PHYS", null, null);
        ExistingDepartment(department);

        Result result = await _delete.Handle(
            new DeleteDepartmentCommand(department.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _departments.Received(1).Remove(department);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deleting_a_department_that_is_gone_is_reported_as_not_found()
    {
        _departments.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Department?)null);

        Result result = await _delete.Handle(
            new DeleteDepartmentCommand(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(DepartmentErrors.NotFound);
    }
}
