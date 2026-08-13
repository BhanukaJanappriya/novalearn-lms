import type { AssessmentOverviewItem } from "../api/overviewQueries";

/** The tab a row belongs to. "marking" is a state, the other three are what a row is. */
export type OverviewFilter = "marking" | "all" | "assignments" | "quizzes" | "drafts";

/** How urgent a deadline is, which is what drives the colour on the row. */
export type DueState = "overdue" | "soon" | "later" | "none";

const DUE_SOON_DAYS = 7;

/**
 * Classifies a deadline. Drafts are always "none": nobody can hand one in, so calling it overdue
 * would invent work. This mirrors how the server counts its summary, so the tallies at the top of
 * the page and the badges on the rows cannot tell different stories.
 */
export function dueState(item: AssessmentOverviewItem, now: Date = new Date()): DueState {
  if (item.status === "Draft" || !item.dueAtUtc) return "none";

  const due = new Date(item.dueAtUtc).getTime();
  const elapsed = due - now.getTime();

  if (elapsed < 0) return "overdue";
  if (elapsed <= DUE_SOON_DAYS * 24 * 60 * 60 * 1000) return "soon";
  return "later";
}

/** Short, human phrasing of a deadline: "3 days left", "2 days overdue", "No deadline". */
export function dueLabel(item: AssessmentOverviewItem, now: Date = new Date()): string {
  if (!item.dueAtUtc) return item.kind === "Quiz" ? "Open while published" : "No deadline";

  const due = new Date(item.dueAtUtc);
  const dayMs = 24 * 60 * 60 * 1000;
  const days = Math.round((due.getTime() - now.getTime()) / dayMs);

  if (item.status === "Draft") return `Draft, dated ${due.toLocaleDateString()}`;
  if (days === 0) return "Due today";
  if (days > 0) return `${days} day${days === 1 ? "" : "s"} left`;

  const overdue = Math.abs(days);
  return `${overdue} day${overdue === 1 ? "" : "s"} overdue`;
}

/** Applies the tab and the free text search together. */
export function filterItems(
  items: AssessmentOverviewItem[],
  filter: OverviewFilter,
  search: string,
): AssessmentOverviewItem[] {
  const needle = search.trim().toLowerCase();

  return items.filter((item) => {
    const matchesFilter =
      filter === "all" ||
      (filter === "marking" && item.awaitingMarkingCount > 0) ||
      (filter === "assignments" && item.kind === "Assignment") ||
      (filter === "quizzes" && item.kind === "Quiz") ||
      (filter === "drafts" && item.status === "Draft");

    if (!matchesFilter) return false;
    if (!needle) return true;

    return (
      item.title.toLowerCase().includes(needle) ||
      item.courseTitle.toLowerCase().includes(needle)
    );
  });
}

/**
 * What share of the class has handed in. Returns null when nobody is enrolled, so the caller can
 * say so rather than rendering a misleading full or empty bar.
 */
export function submissionPercent(item: AssessmentOverviewItem): number | null {
  if (item.enrolledCount <= 0) return null;
  return Math.min(100, Math.round((item.submittedCount / item.enrolledCount) * 100));
}

/** Where the row's "open this" link should go, which differs by kind. */
export function manageHref(item: AssessmentOverviewItem): string {
  return item.kind === "Assignment"
    ? `/admin/courses/${item.courseId}/assignments`
    : `/admin/courses/${item.courseId}/quizzes/${item.id}`;
}

/** Where the row's marking link should go when there is work waiting. */
export function markingHref(item: AssessmentOverviewItem): string {
  return item.kind === "Assignment"
    ? `/admin/courses/${item.courseId}/assignments`
    : `/admin/courses/${item.courseId}/quizzes/${item.id}/results`;
}
