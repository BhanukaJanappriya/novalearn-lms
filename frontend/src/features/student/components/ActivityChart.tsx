import type { ActivityPoint } from "../api/types";

interface ActivityChartProps {
  points: ActivityPoint[];
}

/**
 * Six months of enrolment activity as CSS bars. Hand-rolled rather than Recharts on purpose:
 * this page loads for every student and the chart library is a 450 kB dependency.
 */
export function ActivityChart({ points }: ActivityChartProps) {
  const peak = Math.max(1, ...points.map((p) => p.value));
  const total = points.reduce((sum, p) => sum + p.value, 0);

  if (total === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No enrolments in the last six months. The catalog is a good place to start.
      </p>
    );
  }

  return (
    <div>
      <div className="flex h-28 items-end gap-2" role="list" aria-label="Enrolments per month">
        {points.map((point) => (
          <div
            key={point.label}
            role="listitem"
            aria-label={`${point.label}: ${point.value} enrolment${point.value === 1 ? "" : "s"}`}
            className="group flex flex-1 flex-col items-center justify-end gap-1.5"
          >
            <span className="text-xs font-semibold tabular-nums text-muted-foreground opacity-0 transition-opacity group-hover:opacity-100">
              {point.value}
            </span>
            <div
              className="w-full rounded-t-md bg-primary/80 transition-[height,background-color] duration-500 group-hover:bg-primary"
              style={{ height: `${Math.max(4, (point.value / peak) * 100)}%` }}
            />
          </div>
        ))}
      </div>
      <div className="mt-2 flex gap-2">
        {points.map((point) => (
          <span key={point.label} className="flex-1 text-center text-xs text-muted-foreground">
            {point.label}
          </span>
        ))}
      </div>
    </div>
  );
}
