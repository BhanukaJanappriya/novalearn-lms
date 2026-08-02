import type { Assignment, CellStatus, SubmissionStatus } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const statusVariant: Record<SubmissionStatus, BadgeVariant> = {
  Submitted: "warning",
  Graded: "success",
};

export const cellVariant: Record<CellStatus, BadgeVariant> = {
  Missing: "neutral",
  Submitted: "warning",
  Graded: "success",
};

/** Formats a due date, or a dash when the work is open ended. */
export function formatDue(iso: string | null): string {
  if (!iso) return "No due date";
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

/** Converts an ISO instant into the value a datetime-local input expects. */
export function toLocalInputValue(iso: string | null): string {
  if (!iso) return "";
  const date = new Date(iso);
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

/** Converts a datetime-local input value back into an ISO instant, or null when cleared. */
export function fromLocalInputValue(value: string): string | null {
  return value ? new Date(value).toISOString() : null;
}

/** Short human status for a learner looking at their own assignment row. */
export function learnerStatus(assignment: Assignment): {
  label: string;
  variant: BadgeVariant;
} {
  const submission = assignment.mySubmission;

  if (submission?.status === "Graded") {
    return { label: `${submission.pointsAwarded}/${assignment.maxPoints}`, variant: "success" };
  }
  if (submission) {
    return { label: "Submitted", variant: "warning" };
  }
  if (!assignment.isOpen) {
    return { label: "Closed", variant: "destructive" };
  }
  return { label: "Not started", variant: "neutral" };
}

/** Whether a due date has passed, used to highlight overdue work. */
export function isOverdue(assignment: Assignment): boolean {
  return Boolean(assignment.dueAtUtc) && new Date(assignment.dueAtUtc!) < new Date();
}
