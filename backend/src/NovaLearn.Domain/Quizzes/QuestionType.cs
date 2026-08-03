namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// The kinds of question a quiz can ask. All three mark automatically, which is the point of
/// quizzes as distinct from assignments.
/// </summary>
public enum QuestionType
{
    /// <summary>Several options, exactly one correct.</summary>
    MultipleChoice,

    /// <summary>A statement the learner marks true or false.</summary>
    TrueFalse,

    /// <summary>Free text, matched case-insensitively against a list of accepted answers.</summary>
    ShortAnswer
}
