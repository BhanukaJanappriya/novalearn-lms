import { motion } from "framer-motion";
import { ArrowDownRight, ArrowUpRight, Minus } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { cn } from "@/lib/utils";
import { formatKpi } from "@/lib/format";
import { useCountUp } from "@/hooks/useCountUp";
import { useSpotlight } from "@/hooks/useSpotlight";
import type { KpiMetric } from "../api/types";
import { iconRegistry } from "./iconRegistry";
import { Sparkline } from "./Sparkline";

const trendIcon = { up: ArrowUpRight, down: ArrowDownRight, flat: Minus };

export function KpiCard({ metric }: { metric: KpiMetric }) {
  const navigate = useNavigate();
  const animated = useCountUp(metric.value);
  const { ref, onMouseMove } = useSpotlight<HTMLButtonElement>();
  const Icon = iconRegistry[metric.icon];
  const TrendIcon = trendIcon[metric.trend];

  // A positive delta is "good" unless the metric is one where lower is better.
  const isGood =
    metric.trend === "flat"
      ? null
      : metric.higherIsBetter
        ? metric.trend === "up"
        : metric.trend === "down";

  const interactive = Boolean(metric.href);

  return (
    <motion.button
      ref={ref}
      onMouseMove={onMouseMove}
      type="button"
      disabled={!interactive}
      onClick={() => metric.href && navigate(metric.href)}
      whileHover={{ y: -4 }}
      transition={{ type: "spring", stiffness: 320, damping: 22 }}
      title={metric.hint ?? metric.label}
      className={cn(
        "spotlight group flex w-full flex-col gap-3 rounded-[18px] border border-border bg-card p-5 text-left shadow-soft",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background",
        interactive ? "cursor-pointer hover:shadow-lg hover:border-primary/30" : "cursor-default",
      )}
    >
      <div className="flex items-start justify-between">
        <span
          className="flex h-11 w-11 items-center justify-center rounded-2xl text-white shadow-sm"
          style={{ backgroundImage: `linear-gradient(135deg, ${metric.accent[0]}, ${metric.accent[1]})` }}
        >
          <Icon className="h-5 w-5" />
        </span>
        <span
          className={cn(
            "inline-flex items-center gap-0.5 rounded-full px-2 py-0.5 text-xs font-semibold",
            isGood === null && "bg-muted text-muted-foreground",
            isGood === true && "bg-success/10 text-success",
            isGood === false && "bg-destructive/10 text-destructive",
          )}
        >
          <TrendIcon className="h-3 w-3" />
          {Math.abs(metric.deltaPct).toFixed(1)}%
        </span>
      </div>

      <div>
        <div className="text-2xl font-semibold tracking-tight tabular-nums">
          {formatKpi(animated, metric.format)}
        </div>
        <div className="mt-0.5 text-sm text-muted-foreground">{metric.label}</div>
      </div>

      <div className="mt-auto">
        <Sparkline data={metric.spark} color={metric.accent[0]} className="w-full" width={220} height={34} />
      </div>
    </motion.button>
  );
}
