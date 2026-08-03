import type { QuestionType, QuizSummary } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const questionTypeLabels: Record<QuestionType, string> = {
  MultipleChoice: "Multiple choice",
  TrueFalse: "True or false",
  ShortAnswer: "Short answer",
};

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

/** Two default options, so a new true or false question is usable immediately. */
export function defaultOptionsFor(type: QuestionType): { text: string; isCorrect: boolean }[] {
  if (type === "TrueFalse") {
    return [
      { text: "True", isCorrect: true },
      { text: "False", isCorrect: false },
    ];
  }
  return [
    { text: "", isCorrect: true },
    { text: "", isCorrect: false },
  ];
}
