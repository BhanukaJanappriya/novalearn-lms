/**
 * Chart colour system.
 *
 * The categorical palette is the data-viz skill's validated reference set (fixed
 * slot order = the CVD-safety mechanism — do not reorder). Sequential/brand marks
 * use the NovaLearn purple. Both modes are hand-stepped, not auto-flipped.
 */

export interface ChartTheme {
  /** Fixed-order categorical hues (identity encoding). */
  categorical: string[];
  /** Brand purple for single-series area/bar marks. */
  brand: string;
  brandSoft: string;
  grid: string;
  axis: string;
  tooltipBg: string;
  tooltipBorder: string;
  text: string;
  textMuted: string;
  status: {
    good: string;
    warning: string;
    serious: string;
    critical: string;
  };
}

const light: ChartTheme = {
  categorical: ["#2a78d6", "#1baf7a", "#eda100", "#008300", "#4a3aa7", "#e34948"],
  brand: "#8B5CF6",
  brandSoft: "#A78BFA",
  grid: "#e2e8f0",
  axis: "#94a3b8",
  tooltipBg: "#ffffff",
  tooltipBorder: "rgba(15,23,42,0.10)",
  text: "#1F2937",
  textMuted: "#64748b",
  status: { good: "#0ca30c", warning: "#fab219", serious: "#ec835a", critical: "#d03b3b" },
};

const dark: ChartTheme = {
  categorical: ["#3987e5", "#199e70", "#c98500", "#008300", "#9085e9", "#e66767"],
  brand: "#A78BFA",
  brandSoft: "#8B5CF6",
  grid: "#1e293b",
  axis: "#475569",
  tooltipBg: "#0f172a",
  tooltipBorder: "rgba(255,255,255,0.10)",
  text: "#e2e8f0",
  textMuted: "#94a3b8",
  status: { good: "#0ca30c", warning: "#fab219", serious: "#ec835a", critical: "#d03b3b" },
};

export function getChartTheme(isDark: boolean): ChartTheme {
  return isDark ? dark : light;
}
