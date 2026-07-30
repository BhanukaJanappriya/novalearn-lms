import { cn } from "@/lib/utils";

export interface ProgressProps {
  /** Completion percentage; values outside 0..100 are clamped. */
  value: number;
  /** Accessible name, e.g. the course title the bar belongs to. */
  label?: string;
  size?: "sm" | "md";
  /** Overrides the bar colour (defaults to brand purple, green once complete). */
  indicatorClassName?: string;
  className?: string;
}

/** Slim, theme-aware progress bar. */
export function Progress({ value, label, size = "md", indicatorClassName, className }: ProgressProps) {
  const pct = Math.min(100, Math.max(0, Math.round(value)));

  return (
    <div
      role="progressbar"
      aria-valuenow={pct}
      aria-valuemin={0}
      aria-valuemax={100}
      aria-label={label ? `${label} progress` : "Progress"}
      className={cn(
        "w-full overflow-hidden rounded-full bg-muted",
        size === "sm" ? "h-1.5" : "h-2",
        className,
      )}
    >
      <div
        className={cn(
          "h-full rounded-full transition-[width] duration-500 ease-out",
          indicatorClassName ?? (pct >= 100 ? "bg-success" : "bg-primary"),
        )}
        style={{ width: `${pct}%` }}
      />
    </div>
  );
}
