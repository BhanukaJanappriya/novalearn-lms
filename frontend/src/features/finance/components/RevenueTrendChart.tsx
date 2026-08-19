import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useIsDark } from "@/hooks/useIsDark";
import { ChartTooltip } from "@/features/admin/components/charts/ChartTooltip";
import { getChartTheme } from "@/features/admin/theme/chartTheme";
import type { AnalyticsGranularity } from "@/features/analytics/api/queries";
import type { RevenuePoint } from "../api/queries";
import { formatMoney } from "../lib/finance";

interface RevenueTrendChartProps {
  data: RevenuePoint[];
  granularity: AnalyticsGranularity;
  currency: string;
}

function bucketLabel(date: string, granularity: AnalyticsGranularity): string {
  const parsed = new Date(date);

  return granularity === "Month"
    ? parsed.toLocaleDateString(undefined, { month: "short", year: "2-digit" })
    : parsed.toLocaleDateString(undefined, { day: "numeric", month: "short" });
}

/** Net revenue per bucket. See the backend's finance read model for what "net" means here. */
export function RevenueTrendChart({ data, granularity, currency }: RevenueTrendChartProps) {
  const theme = getChartTheme(useIsDark());

  const shaped = data.map((point) => ({
    label: bucketLabel(point.date, granularity),
    Revenue: point.revenue,
  }));

  return (
    <ResponsiveContainer width="100%" height={280}>
      <AreaChart data={shaped} margin={{ top: 8, right: 8, left: 4, bottom: 0 }}>
        <defs>
          <linearGradient id="grad-revenue" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={theme.status.good} stopOpacity={0.35} />
            <stop offset="100%" stopColor={theme.status.good} stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke={theme.grid} vertical={false} />
        <XAxis
          dataKey="label"
          stroke={theme.axis}
          tickLine={false}
          axisLine={false}
          fontSize={11}
          minTickGap={24}
        />
        <YAxis
          stroke={theme.axis}
          tickLine={false}
          axisLine={false}
          fontSize={11}
          width={56}
          tickFormatter={(value: number) => formatMoney(value, currency)}
        />
        <Tooltip content={<ChartTooltip />} cursor={{ stroke: theme.axis, strokeDasharray: "3 3" }} />
        <Area
          type="monotone"
          dataKey="Revenue"
          stroke={theme.status.good}
          strokeWidth={2}
          fill="url(#grad-revenue)"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}
