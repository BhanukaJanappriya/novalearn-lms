import { getChartTheme } from "../../theme/chartTheme";
import { useIsDark } from "@/hooks/useIsDark";
import { formatNumber } from "@/lib/format";

/** Recharts v3 passes these to a custom tooltip `content` element. */
interface TooltipPayloadEntry {
  dataKey?: string | number;
  name?: string | number;
  value?: number | string;
  color?: string;
}

interface ChartTooltipProps {
  active?: boolean;
  label?: string | number;
  payload?: TooltipPayloadEntry[];
}

/** Themed tooltip shared by every Recharts chart (crosshair + per-mark hover). */
export function ChartTooltip({ active, payload, label }: ChartTooltipProps) {
  const theme = getChartTheme(useIsDark());
  if (!active || !payload?.length) return null;

  return (
    <div
      className="rounded-lg border px-3 py-2 text-xs shadow-lg"
      style={{ background: theme.tooltipBg, borderColor: theme.tooltipBorder, color: theme.text }}
    >
      {label != null && <div className="mb-1 font-semibold">{label}</div>}
      <div className="space-y-0.5">
        {payload.map((entry, i) => (
          <div key={entry.dataKey ?? i} className="flex items-center gap-2">
            <span className="h-2 w-2 rounded-full" style={{ background: entry.color }} />
            <span style={{ color: theme.textMuted }}>{entry.name}</span>
            <span className="ml-auto font-medium tabular-nums">{formatNumber(Number(entry.value))}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
