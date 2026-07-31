using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Student.Dashboard;

/// <summary>Builds the dashboard for the calling learner. Takes no parameters by design.</summary>
public sealed record GetStudentDashboardQuery : IRequest<Result<StudentDashboardResponse>>;
