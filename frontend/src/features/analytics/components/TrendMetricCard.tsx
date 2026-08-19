import { ArrowDownRight, ArrowUpRight, Minus, type LucideIcon } from "lucide-react";
import type { TrendMetric } from "../api/queries";

interface TrendMetricCardProps {
  label: string;
  metric: TrendMetric;
  icon: LucideIcon;
  /** Appended to the current value, for figures that are percentages. */
  suffix?: string;
  /** Overrides how the current value is rendered — for currency, with its own symbol and decimals. */
  formatValue?: (value: number) => string;
  /**
   * Whether a rise is good news. True for everything here today, but stated rather than assumed
   * so a future metric like dropouts cannot inherit the wrong colour by default.
   */
  riseIsGood?: boolean;
}

/** One headline figure, with how it compares to the window before. */
export function TrendMetricCard({
  label,
  metric,
  icon: Icon,
  suffix = "",
  formatValue,
  riseIsGood = true,
}: TrendMetricCardProps) {
  const change = metric.changePercent;
  const flat = change === 0;
  const rising = change !== null && change > 0;

  const tone =
    change === null || flat
      ? "text-muted-foreground"
      : rising === riseIsGood
        ? "text-success"
        : "text-destructive";

  const ChangeIcon = change === null || flat ? Minus : rising ? ArrowUpRight : ArrowDownRight;

  return (
    <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
      <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
        <Icon className="h-3.5 w-3.5" aria-hidden />
        {label}
      </p>

      <p className="mt-1 text-2xl font-semibold tabular-nums">
        {formatValue ? formatValue(metric.current) : Math.round(metric.current * 10) / 10}
        {suffix}
      </p>

      <p className={`mt-1 flex items-center gap-1 text-xs ${tone}`}>
        <ChangeIcon className="h-3.5 w-3.5" aria-hidden />
        {change === null ? (
          // Saying so beats rendering an infinite rise the first time anything is recorded.
          <span>No comparison yet</span>
        ) : (
          <span>
            {Math.abs(change)}% vs previous
          </span>
        )}
      </p>
    </div>
  );
}
