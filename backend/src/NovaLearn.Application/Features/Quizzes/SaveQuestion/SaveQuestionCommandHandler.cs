using MediatR;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Quizzes.SaveQuestion;

public sealed class SaveQuestionCommandHandler(
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<SaveQuestionCommand, Result<AuthoringQuestionDto>>
{
    public async Task<Result<AuthoringQuestionDto>> Handle(
        SaveQuestionCommand request, CancellationToken cancellationToken)
    {
        Quiz? quiz = await quizzes.GetQuizWithQuestionsAsync(request.QuizId, cancellationToken);
        if (quiz is null)
        {
            return Result.Failure<AuthoringQuestionDto>(QuizErrors.QuizNotFound);
        }

        if (QuizAuthority.CheckCanManage(quiz.Course, currentUser) is { } denied)
        {
            return Result.Failure<AuthoringQuestionDto>(denied);
        }

        return request.QuestionId is { } questionId
            ? await UpdateAsync(quiz, questionId, request, cancellationToken)
            : await CreateAsync(quiz, request, cancellationToken);
    }

    private async Task<Result<AuthoringQuestionDto>> CreateAsync(
        Quiz quiz, SaveQuestionCommand request, CancellationToken cancellationToken)
    {
        int sortOrder = quiz.Questions.Count == 0 ? 0 : quiz.Questions.Max(q => q.SortOrder) + 1;

        Question question = quiz.AddQuestion(
            request.Text, request.Type, request.Points, sortOrder, request.AcceptedAnswers,
            request.IsRequired, request.MarkingGuidance);

        IReadOnlyList<QuestionOption> options = question.ReplaceOptions(
            request.Options.Select(o => (o.Text, o.IsCorrect)));

        // Stated explicitly: BaseEntity assigns the key client-side, so entities reached only
        // through a navigation are tracked as Modified and save as no-op UPDATEs.
        await quizzes.AddQuestionAsync(question, cancellationToken);
        await quizzes.ReplaceOptionsAsync(question, options, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthoringQuestionDto.FromEntity(question);
    }

    private async Task<Result<AuthoringQuestionDto>> UpdateAsync(
        Quiz quiz, Guid questionId, SaveQuestionCommand request, CancellationToken cancellationToken)
    {
        Question? question = await quizzes.GetQuestionAsync(questionId, cancellationToken);

        // Checking the parent as well as existence stops a question being moved between quizzes
        // by passing someone else's id.
        if (question is null || question.QuizId != quiz.Id)
        {
            return Result.Failure<AuthoringQuestionDto>(QuizErrors.QuestionNotFound);
        }

        question.Update(
            request.Text, request.Type, request.Points, request.AcceptedAnswers,
            request.IsRequired, request.MarkingGuidance);

        IReadOnlyList<QuestionOption> options = question.ReplaceOptions(
            request.Options.Select(o => (o.Text, o.IsCorrect)));

        await quizzes.ReplaceOptionsAsync(question, options, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthoringQuestionDto.FromEntity(question);
    }
}
