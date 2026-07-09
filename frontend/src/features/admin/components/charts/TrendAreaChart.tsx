import { Area, AreaChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useIsDark } from "@/hooks/useIsDark";
import { getChartTheme } from "../../theme/chartTheme";
import type { TimeSeriesPoint } from "../../api/types";
import { ChartTooltip } from "./ChartTooltip";

interface Props {
  data: TimeSeriesPoint[];
  /** Whether to render the `compare` series (same scale — never a 2nd y-axis). */
  showCompare?: boolean;
}

/** Enrollment / registration trend. Single y-axis; two same-scale series. */
export function TrendAreaChart({ data, showCompare = true }: Props) {
  const theme = getChartTheme(useIsDark());

  return (
    <ResponsiveContainer width="100%" height={260}>
      <AreaChart data={data} margin={{ top: 8, right: 8, left: -12, bottom: 0 }}>
        <defs>
          <linearGradient id="grad-current" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={theme.brand} stopOpacity={0.35} />
            <stop offset="100%" stopColor={theme.brand} stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke={theme.grid} vertical={false} />
        <XAxis dataKey="label" stroke={theme.axis} tickLine={false} axisLine={false} fontSize={11} />
        <YAxis stroke={theme.axis} tickLine={false} axisLine={false} fontSize={11} width={44} />
        <Tooltip content={<ChartTooltip />} cursor={{ stroke: theme.axis, strokeDasharray: "3 3" }} />
        <Legend iconType="circle" wrapperStyle={{ fontSize: 12, color: theme.textMuted }} />
        {showCompare && (
          <Area
            type="monotone"
            dataKey="compare"
            name="Previous year"
            stroke={theme.axis}
            strokeWidth={2}
            strokeDasharray="4 4"
            fill="transparent"
            dot={false}
          />
        )}
        <Area
          type="monotone"
          dataKey="value"
          name="This year"
          stroke={theme.brand}
          strokeWidth={2.5}
          fill="url(#grad-current)"
          dot={false}
          activeDot={{ r: 4, strokeWidth: 2, stroke: theme.tooltipBg }}
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}
