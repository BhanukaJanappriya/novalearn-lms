/** Short relative time until a future instant, e.g. "in 12m", "in 2h". Past instants read "now". */
export function timeUntil(iso: string): string {
  const diffMs = new Date(iso).getTime() - Date.now();
  if (diffMs <= 0) return "now";
  const mins = Math.round(diffMs / 60000);
  if (mins < 1) return "in under a minute";
  if (mins < 60) return `in ${mins}m`;
  const hrs = Math.round(mins / 60);
  if (hrs < 24) return `in ${hrs}h`;
  const days = Math.round(hrs / 24);
  return `in ${days}d`;
}
