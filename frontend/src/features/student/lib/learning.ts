/** Formats a minute total as "4h 20m", "45m", or a dash when there is nothing to show. */
export function formatDuration(minutes: number): string {
  if (minutes <= 0) return "—";

  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;

  if (hours === 0) return `${rest}m`;
  if (rest === 0) return `${hours}h`;
  return `${hours}h ${rest}m`;
}

/** Encouraging one-liner keyed off overall progress. Purely presentational. */
export function progressHeadline(activeCourses: number, averageProgress: number): string {
  if (activeCourses === 0) return "Pick a course to get started.";
  if (averageProgress >= 80) return "You are almost there. Keep going.";
  if (averageProgress >= 40) return "Good momentum. Keep it up.";
  return "Every lesson counts. Pick up where you left off.";
}

/** Rounds to the nearest step, used by the quick progress control. */
export function snapToStep(value: number, step = 5): number {
  return Math.min(100, Math.max(0, Math.round(value / step) * step));
}
