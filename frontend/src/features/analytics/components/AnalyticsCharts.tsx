import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { useIsDark } from "@/hooks/useIsDark";
import { ChartTooltip } from "@/features/admin/components/charts/ChartTooltip";
import { getChartTheme } from "@/features/admin/theme/chartTheme";
import type { AnalyticsGranularity, AnalyticsPoint, ScoreBucket } from "../api/queries";

/** Formats a bucket's date for the axis, at the resolution the bucket represents. */
function bucketLabel(date: string, granularity: AnalyticsGranularity): string {
  const parsed = new Date(date);

  if (granularity === "Month") {
    return parsed.toLocaleDateString(undefined, { month: "short", year: "2-digit" });
  }

  return parsed.toLocaleDateString(undefined, { day: "numeric", month: "short" });
}

interface EnrolmentTrendChartProps {
  data: AnalyticsPoint[];
  granularity: AnalyticsGranularity;
}

/**
 * Enrolments against completions over the window.
 *
 * Both are counts of people on the same scale, so they share one axis. A second axis would let
 * two unrelated magnitudes be drawn as though they were tracking each other.
 */
export function EnrolmentTrendChart({ data, granularity }: EnrolmentTrendChartProps) {
  const theme = getChartTheme(useIsDark());

  const shaped = data.map((point) => ({
    label: bucketLabel(point.date, granularity),
    Enrolments: point.enrolments,
    Completions: point.completions,
  }));

  return (
    <ResponsiveContainer width="100%" height={280}>
      <AreaChart data={shaped} margin={{ top: 8, right: 8, left: -12, bottom: 0 }}>
        <defs>
          <linearGradient id="grad-enrolments" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={theme.brand} stopOpacity={0.35} />
            <stop offset="100%" stopColor={theme.brand} stopOpacity={0} />
          </linearGradient>
          <linearGradient id="grad-completions" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={theme.status.good} stopOpacity={0.3} />
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
          width={40}
          allowDecimals={false}
        />
        <Tooltip content={<ChartTooltip />} cursor={{ stroke: theme.axis, strokeDasharray: "3 3" }} />
        <Legend iconType="circle" wrapperStyle={{ fontSize: 12, color: theme.textMuted }} />
        <Area
          type="monotone"
          dataKey="Enrolments"
          stroke={theme.brand}
          strokeWidth={2}
          fill="url(#grad-enrolments)"
        />
        <Area
          type="monotone"
          dataKey="Completions"
          stroke={theme.status.good}
          strokeWidth={2}
          fill="url(#grad-completions)"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

/**
 * How marks landed across bands.
 *
 * Coloured by band rather than by one brand hue, so a result that is mostly failures reads as a
 * problem at a glance instead of as a pleasant purple bar.
 */
export function ScoreDistributionChart({ data }: { data: ScoreBucket[] }) {
  const theme = getChartTheme(useIsDark());

  const colours = [
    theme.status.critical,
    theme.status.serious,
    theme.status.warning,
    theme.categorical[0],
    theme.status.good,
    theme.status.good,
  ];

  return (
    <ResponsiveContainer width="100%" height={240}>
      <BarChart data={data} margin={{ top: 8, right: 8, left: -12, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke={theme.grid} vertical={false} />
        <XAxis dataKey="label" stroke={theme.axis} tickLine={false} axisLine={false} fontSize={11} />
        <YAxis
          stroke={theme.axis}
          tickLine={false}
          axisLine={false}
          fontSize={11}
          width={40}
          allowDecimals={false}
        />
        <Tooltip content={<ChartTooltip />} cursor={{ fill: theme.grid, fillOpacity: 0.3 }} />
        <Bar dataKey="count" name="Results" radius={[6, 6, 0, 0]}>
          {data.map((bucket, index) => (
            <Cell key={bucket.label} fill={colours[index] ?? theme.brand} />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );
}
