using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.DuplicateQuestion;

/// <summary>
/// Copies a question, answer key and all, to the end of its quiz. Authoring a bank of similar
/// questions is the common case, and retyping the options every time is the tedious part.
/// </summary>
public sealed record DuplicateQuestionCommand(Guid QuestionId) : IRequest<Result<AuthoringQuestionDto>>;

public sealed class DuplicateQuestionCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DuplicateQuestionCommand, Result<AuthoringQuestionDto>>
{
    public async Task<Result<AuthoringQuestionDto>> Handle(
        DuplicateQuestionCommand request, CancellationToken cancellationToken)
    {
        Question? source = await quizzes.GetQuestionAsync(request.QuestionId, cancellationToken);
        if (source is null)
        {
            return Result.Failure<AuthoringQuestionDto>(QuizErrors.QuestionNotFound);
        }

        if (QuizAuthority.CheckCanManage(source.Quiz?.Course, currentUser) is { } denied)
        {
            return Result.Failure<AuthoringQuestionDto>(denied);
        }

        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(source.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<AuthoringQuestionDto>(QuizErrors.QuizNotFound);
        }

        int sortOrder = quiz.Questions.Count == 0 ? 0 : quiz.Questions.Max(q => q.SortOrder) + 1;

        Question copy = quiz.AddQuestion(
            $"{source.Text} (copy)",
            source.Type,
            source.Points,
            sortOrder,
            source.AcceptedAnswerList,
            source.IsRequired,
            source.MarkingGuidance);

        IReadOnlyList<QuestionOption> options = copy.ReplaceOptions(
            source.Options.OrderBy(o => o.SortOrder).Select(o => (o.Text, o.IsCorrect)));

        // Stated explicitly: BaseEntity assigns the key client-side, so entities reached only
        // through a navigation are tracked as Modified and save as no-op UPDATEs.
        await quizzes.AddQuestionAsync(copy, cancellationToken);
        await quizzes.ReplaceOptionsAsync(copy, options, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthoringQuestionDto.FromEntity(copy);
    }
}
