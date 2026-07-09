import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import { useIsDark } from "@/hooks/useIsDark";
import { getChartTheme } from "../../theme/chartTheme";
import type { CategoryDatum } from "../../api/types";
import { ChartTooltip } from "./ChartTooltip";
import { formatNumber } from "@/lib/format";

/**
 * Role distribution. Categorical palette in fixed slot order; identity carried by
 * a legend + direct % labels (the relief rule — colour is never alone here).
 */
export function RoleDonutChart({ data }: { data: CategoryDatum[] }) {
  const theme = getChartTheme(useIsDark());
  const total = data.reduce((sum, d) => sum + d.value, 0);

  return (
    <div className="flex flex-col items-center gap-4 sm:flex-row">
      <div className="relative h-[200px] w-[200px] shrink-0">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Tooltip content={<ChartTooltip />} />
            <Pie
              data={data}
              dataKey="value"
              nameKey="label"
              innerRadius={62}
              outerRadius={92}
              paddingAngle={2}
              stroke={theme.tooltipBg}
              strokeWidth={2}
            >
              {data.map((_, i) => (
                <Cell key={i} fill={theme.categorical[i % theme.categorical.length]} />
              ))}
            </Pie>
          </PieChart>
        </ResponsiveContainer>
        <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
          <span className="text-xl font-semibold tabular-nums">{formatNumber(total, true)}</span>
          <span className="text-xs text-muted-foreground">Total users</span>
        </div>
      </div>

      <ul className="flex-1 space-y-2">
        {data.map((d, i) => (
          <li key={d.label} className="flex items-center gap-2 text-sm">
            <span
              className="h-2.5 w-2.5 shrink-0 rounded-full"
              style={{ background: theme.categorical[i % theme.categorical.length] }}
            />
            <span className="text-muted-foreground">{d.label}</span>
            <span className="ml-auto font-medium tabular-nums">
              {((d.value / total) * 100).toFixed(1)}%
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
