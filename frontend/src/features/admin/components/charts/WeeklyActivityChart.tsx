import { Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useIsDark } from "@/hooks/useIsDark";
import { getChartTheme } from "../../theme/chartTheme";
import type { TimeSeriesPoint } from "../../api/types";
import { ChartTooltip } from "./ChartTooltip";

/** Weekly active-users bars. Single-hue brand; weekend bars recede in tone. */
export function WeeklyActivityChart({ data }: { data: TimeSeriesPoint[] }) {
  const theme = getChartTheme(useIsDark());
  const peak = Math.max(...data.map((d) => d.value));

  return (
    <ResponsiveContainer width="100%" height={240}>
      <BarChart data={data} margin={{ top: 8, right: 8, left: -12, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke={theme.grid} vertical={false} />
        <XAxis dataKey="label" stroke={theme.axis} tickLine={false} axisLine={false} fontSize={11} />
        <YAxis stroke={theme.axis} tickLine={false} axisLine={false} fontSize={11} width={44} />
        <Tooltip content={<ChartTooltip />} cursor={{ fill: theme.grid, opacity: 0.4 }} />
        <Bar dataKey="value" name="Active users" radius={[4, 4, 0, 0]} maxBarSize={40}>
          {data.map((d, i) => (
            <Cell key={i} fill={d.value === peak ? theme.brand : theme.brandSoft} fillOpacity={d.value === peak ? 1 : 0.55} />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );
}
