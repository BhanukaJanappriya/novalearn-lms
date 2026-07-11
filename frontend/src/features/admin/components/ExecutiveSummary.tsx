import { motion } from "framer-motion";
import { Activity, CalendarRange, CheckCircle2, ClipboardList, Users } from "lucide-react";
import { formatNumber } from "@/lib/format";
import { useSpotlight } from "@/hooks/useSpotlight";
import type { ExecutiveSummary as SummaryData } from "../api/types";

/** Executive hero: greeting + semester progress ring + at-a-glance status tiles. */
export function ExecutiveSummary({ data, adminName }: { data: SummaryData; adminName: string }) {
  const { ref, onMouseMove } = useSpotlight<HTMLDivElement>();
  const tiles = [
    { icon: Users, label: "Active now", value: formatNumber(data.activeUsersNow), tint: "text-primary" },
    { icon: Activity, label: "Server health", value: `${data.serverHealthPct}%`, tint: "text-success" },
    { icon: ClipboardList, label: "Pending approvals", value: String(data.pendingApprovals), tint: "text-[hsl(var(--warning))]" },
    { icon: CalendarRange, label: "Academic period", value: `Week 9 / 16`, tint: "text-accent-foreground" },
  ];

  return (
    <div
      ref={ref}
      onMouseMove={onMouseMove}
      className="spotlight relative overflow-hidden rounded-[18px] border border-border bg-gradient-to-br from-primary/10 via-card to-accent/10 p-6 shadow-soft"
    >
      <div className="pointer-events-none absolute -right-16 -top-16 h-56 w-56 rounded-full bg-primary/10 blur-3xl" />
      <div className="relative z-[2] flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex items-center gap-5">
          <ProgressRing value={data.semesterProgressPct} />
          <div>
            <div className="inline-flex items-center gap-1.5 rounded-full bg-success/10 px-2.5 py-0.5 text-xs font-medium text-success">
              <CheckCircle2 className="h-3 w-3" />
              All systems operational
            </div>
            <h1 className="mt-2 text-2xl font-semibold tracking-tight">Welcome back, {adminName}</h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {data.semesterName} is <span className="font-medium text-foreground">{data.semesterProgressPct}%</span> complete · {data.academicPeriod}
            </p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4 lg:max-w-md">
          {tiles.map((tile, i) => {
            const Icon = tile.icon;
            return (
              <motion.div
                key={tile.label}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.05 * i }}
                className="rounded-xl border border-border bg-card/70 p-3 backdrop-blur"
              >
                <Icon className={`h-4 w-4 ${tile.tint}`} />
                <p className="mt-2 text-lg font-semibold leading-none tabular-nums">{tile.value}</p>
                <p className="mt-1 text-[11px] text-muted-foreground">{tile.label}</p>
              </motion.div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

function ProgressRing({ value }: { value: number }) {
  const radius = 34;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (value / 100) * circumference;

  return (
    <div className="relative h-20 w-20 shrink-0">
      <svg className="h-20 w-20 -rotate-90" viewBox="0 0 80 80">
        <circle cx="40" cy="40" r={radius} fill="none" stroke="hsl(var(--muted))" strokeWidth="7" />
        <motion.circle
          cx="40"
          cy="40"
          r={radius}
          fill="none"
          stroke="hsl(var(--primary))"
          strokeWidth="7"
          strokeLinecap="round"
          strokeDasharray={circumference}
          initial={{ strokeDashoffset: circumference }}
          animate={{ strokeDashoffset: offset }}
          transition={{ duration: 1, ease: "easeOut" }}
        />
      </svg>
      <div className="absolute inset-0 flex items-center justify-center">
        <span className="text-lg font-semibold tabular-nums">{value}%</span>
      </div>
    </div>
  );
}
