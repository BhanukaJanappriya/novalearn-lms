import { formatNumber } from "@/lib/format";
import { useIsDark } from "@/hooks/useIsDark";
import { getChartTheme } from "../../theme/chartTheme";
import type { CategoryDatum } from "../../api/types";

/**
 * Horizontal ranked bars with direct value labels — hand-rolled so labels sit
 * exactly on each bar (magnitude comparison, single sequential hue).
 */
export function PopularCoursesChart({ data }: { data: CategoryDatum[] }) {
  const theme = getChartTheme(useIsDark());
  const max = Math.max(...data.map((d) => d.value));

  return (
    <ul className="space-y-3">
      {data.map((d) => (
        <li key={d.label}>
          <div className="mb-1 flex items-center justify-between gap-2 text-sm">
            <span className="truncate text-foreground">{d.label}</span>
            <span className="shrink-0 font-medium tabular-nums text-muted-foreground">
              {formatNumber(d.value)}
            </span>
          </div>
          <div className="h-2.5 overflow-hidden rounded-full" style={{ background: theme.grid }}>
            <div
              className="h-full rounded-full transition-all duration-700"
              style={{
                width: `${(d.value / max) * 100}%`,
                backgroundImage: `linear-gradient(90deg, ${theme.brandSoft}, ${theme.brand})`,
              }}
            />
          </div>
        </li>
      ))}
    </ul>
  );
}
