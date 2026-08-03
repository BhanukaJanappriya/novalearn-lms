using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.DeleteQuiz;

public sealed record DeleteQuizCommand(Guid QuizId) : IRequest<Result>;
