import type { AttemptStatus, QuestionType, QuizSummary } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const questionTypeLabels: Record<QuestionType, string> = {
  MultipleChoice: "Multiple choice",
  TrueFalse: "True or false",
  MultipleResponse: "Checkboxes",
  ShortAnswer: "Short answer",
  Essay: "Essay",
};

/** One-line explanation of how each type is marked, shown while authoring. */
export const questionTypeHints: Record<QuestionType, string> = {
  MultipleChoice: "One correct option. Marked automatically.",
  TrueFalse: "A statement to judge. Marked automatically.",
  MultipleResponse: "Several correct options. All or nothing, marked automatically.",
  ShortAnswer: "Typed answer matched against your accepted answers. Marked automatically.",
  Essay: "Long form writing. You mark this one by hand.",
};

/** The types answered by picking from a list. */
export function isOptionBased(type: QuestionType): boolean {
  return type === "MultipleChoice" || type === "TrueFalse" || type === "MultipleResponse";
}

export function requiresManualMarking(type: QuestionType): boolean {
  return type === "Essay";
}

/** Formats a time limit, or says it is untimed. */
export function formatTimeLimit(minutes: number | null): string {
  if (minutes === null) return "No time limit";
  if (minutes < 60) return `${minutes} min`;

  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  return rest === 0 ? `${hours}h` : `${hours}h ${rest}m`;
}

/** Formats an attempt allowance. */
export function formatAttempts(max: number | null): string {
  return max === null ? "Unlimited attempts" : `${max} attempt${max === 1 ? "" : "s"}`;
}

/** Counts down to a deadline as mm:ss, clamped at zero. */
export function formatRemaining(msRemaining: number): string {
  const total = Math.max(0, Math.floor(msRemaining / 1000));
  const minutes = Math.floor(total / 60);
  const seconds = total % 60;
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}

export const attemptStatusVariant: Record<AttemptStatus, BadgeVariant> = {
  InProgress: "neutral",
  PendingReview: "warning",
  Graded: "success",
};

export const attemptStatusLabels: Record<AttemptStatus, string> = {
  InProgress: "In progress",
  PendingReview: "Awaiting marking",
  Graded: "Marked",
};

/** How a learner's standing on a quiz should read. */
export function learnerQuizStatus(quiz: QuizSummary): { label: string; variant: BadgeVariant } {
  if (quiz.attemptsUsed === 0) {
    return { label: "Not attempted", variant: "neutral" };
  }
  if (quiz.hasPassed) {
    return { label: `Passed · ${quiz.bestScorePercent}%`, variant: "success" };
  }
  if (quiz.passingScorePercent !== null) {
    return { label: `Best ${quiz.bestScorePercent}%`, variant: "warning" };
  }
  return { label: `Best ${quiz.bestScorePercent}%`, variant: "default" };
}

/** Sensible starting options so a new question is usable immediately. */
export function defaultOptionsFor(type: QuestionType): { text: string; isCorrect: boolean }[] {
  if (type === "TrueFalse") {
    return [
      { text: "True", isCorrect: true },
      { text: "False", isCorrect: false },
    ];
  }
  if (!isOptionBased(type)) {
    return [];
  }
  return [
    { text: "", isCorrect: type === "MultipleChoice" },
    { text: "", isCorrect: false },
  ];
}
