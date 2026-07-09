import type { KpiFormat } from "@/features/admin/api/types";

const compact = new Intl.NumberFormat("en-US", { notation: "compact", maximumFractionDigits: 1 });
const full = new Intl.NumberFormat("en-US");
const currency = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  notation: "compact",
  maximumFractionDigits: 1,
});

export function formatNumber(value: number, useCompact = false): string {
  return (useCompact ? compact : full).format(Math.round(value));
}

/** Formats a KPI value according to its declared format. */
export function formatKpi(value: number, format: KpiFormat): string {
  switch (format) {
    case "percent":
      return `${value.toFixed(1)}%`;
    case "currency":
      return currency.format(value);
    case "bytes":
      return `${value.toFixed(1)} GB`;
    case "number":
    default:
      return value >= 10000 ? compact.format(value) : full.format(Math.round(value));
  }
}

/** Short relative time, e.g. "3m ago", "2h ago", "4d ago". */
export function timeAgo(iso: string): string {
  const diffMs = Date.now() - new Date(iso).getTime();
  const mins = Math.round(diffMs / 60000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.round(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.round(hrs / 24);
  return `${days}d ago`;
}
