using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Payments.Common;
using NovaLearn.Application.Features.Payments.CreateCheckoutSession;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Payments;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Payments;

public sealed class CreateCheckoutSessionCommandHandlerTests
{
    private readonly ICourseRepository _courses = Substitute.For<ICourseRepository>();
    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IFrontendUrls _frontendUrls = Substitute.For<IFrontendUrls>();
    private readonly ISettingsProvider _settings = Substitute.For<ISettingsProvider>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly CreateCheckoutSessionCommandHandler _sut;

    public CreateCheckoutSessionCommandHandlerTests()
    {
        _sut = new CreateCheckoutSessionCommandHandler(
            _courses, _enrollments, _payments, _gateway, _frontendUrls, _settings, _currentUser, _unitOfWork);

        _currentUser.UserId.Returns(_studentId);
        _settings.GetAsync(Arg.Any<CancellationToken>()).Returns(
            new PlatformSettingsSnapshot("NovaLearn", "support@novalearn.local", true, false, null, "usd", 200));
        _frontendUrls.Build(Arg.Any<string>()).Returns(call => "https://app.test" + call.Arg<string>());
        _enrollments.GetActiveAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);
        _gateway.CreateCheckoutSessionAsync(Arg.Any<CheckoutSessionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CheckoutSessionResult("cs_test_1", "https://checkout.stripe.com/cs_test_1"));
    }

    private static Course PaidCourse(decimal price = 50m, CourseStatus status = CourseStatus.Published) =>
        Course.Create(
            "Intro to Programming", "CS101", "Fundamentals", "Computer Science",
            CourseLevel.Beginner, status, price, null, Guid.NewGuid());

    private Task<Result<CheckoutSessionDto>> Act(Course course)
    {
        _courses.GetByIdAsync(course.Id, Arg.Any<CancellationToken>()).Returns(course);
        return _sut.Handle(new CreateCheckoutSessionCommand(course.Id), CancellationToken.None);
    }

    [Fact]
    public async Task An_unauthenticated_caller_cannot_start_checkout()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<CheckoutSessionDto> result = await Act(PaidCourse());

        result.Error.Should().Be(PaymentErrors.Unauthenticated);
    }

    [Fact]
    public async Task A_missing_course_is_reported()
    {
        _courses.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Course?)null);

        Result<CheckoutSessionDto> result = await _sut.Handle(
            new CreateCheckoutSessionCommand(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(PaymentErrors.CourseNotFound);
    }

    [Fact]
    public async Task An_unpublished_course_cannot_be_checked_out()
    {
        Result<CheckoutSessionDto> result = await Act(PaidCourse(status: CourseStatus.Draft));

        result.Error.Should().Be(PaymentErrors.CourseNotPublished);
    }

    [Fact]
    public async Task A_free_course_is_refused_rather_than_charged_zero()
    {
        Result<CheckoutSessionDto> result = await Act(PaidCourse(price: 0m));

        result.Error.Should().Be(PaymentErrors.CourseIsFree);
        await _gateway.DidNotReceive().CreateCheckoutSessionAsync(
            Arg.Any<CheckoutSessionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Already_being_enrolled_is_refused()
    {
        Course course = PaidCourse();
        _enrollments.GetActiveAsync(_studentId, course.Id, Arg.Any<CancellationToken>())
            .Returns(Enrollment.Create(_studentId, course.Id, DateTimeOffset.UtcNow));

        Result<CheckoutSessionDto> result = await Act(course);

        result.Error.Should().Be(PaymentErrors.AlreadyEnrolled);
    }

    [Fact]
    public async Task A_gateway_failure_is_surfaced_without_writing_a_payment_row()
    {
        _gateway.CreateCheckoutSessionAsync(Arg.Any<CheckoutSessionRequest>(), Arg.Any<CancellationToken>())
            .Returns<CheckoutSessionResult>(_ => throw new InvalidOperationException("Stripe is down"));

        Result<CheckoutSessionDto> result = await Act(PaidCourse());

        result.IsFailure.Should().BeTrue();
        await _payments.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_successful_checkout_records_a_pending_payment_and_returns_the_checkout_url()
    {
        Course course = PaidCourse(price: 42m);

        Result<CheckoutSessionDto> result = await Act(course);

        result.IsSuccess.Should().BeTrue();
        result.Value.CheckoutUrl.Should().Be("https://checkout.stripe.com/cs_test_1");

        await _payments.Received(1).AddAsync(
            Arg.Is<Payment>(p =>
                p.StudentId == _studentId
                && p.CourseId == course.Id
                && p.Amount == 42m
                && p.Status == PaymentStatus.Pending
                && p.ProviderCheckoutSessionId == "cs_test_1"),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
