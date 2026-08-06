namespace NovaLearn.Domain.Quizzes;

/// <summary>
/// The kinds of question a quiz can ask.
///
/// Everything except <see cref="Essay"/> carries its own answer key and marks itself the moment
/// an attempt is handed in. An essay has no key that a machine could check, so it is the one
/// type that waits for a person.
/// </summary>
public enum QuestionType
{
    /// <summary>Several options, exactly one correct.</summary>
    MultipleChoice,

    /// <summary>A statement the learner marks true or false.</summary>
    TrueFalse,

    /// <summary>Several options, one or more correct. Marked all or nothing.</summary>
    MultipleResponse,

    /// <summary>Free text, matched case-insensitively against a list of accepted answers.</summary>
    ShortAnswer,

    /// <summary>Long form writing. Carries no answer key and is always marked by hand.</summary>
    Essay
}
