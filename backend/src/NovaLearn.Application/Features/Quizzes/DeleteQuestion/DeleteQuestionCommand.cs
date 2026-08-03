using MediatR;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.DeleteQuestion;

public sealed record DeleteQuestionCommand(Guid QuestionId) : IRequest<Result>;
