import { AlertTriangle, Info, ShieldAlert, ShieldCheck } from "lucide-react";
import { cn } from "@/lib/utils";
import { timeAgo } from "@/lib/format";
import type { SecurityEvent, SecuritySummary } from "../api/types";

const severityMeta = {
  info: { icon: Info, className: "text-muted-foreground bg-muted" },
  warning: { icon: AlertTriangle, className: "text-[hsl(var(--warning))] bg-warning/10" },
  critical: { icon: ShieldAlert, className: "text-destructive bg-destructive/10" },
} satisfies Record<SecurityEvent["severity"], { icon: typeof Info; className: string }>;

function scoreColor(score: number): string {
  if (score >= 80) return "text-success";
  if (score >= 60) return "text-[hsl(var(--warning))]";
  return "text-destructive";
}

export function SecurityPanel({ data }: { data: SecuritySummary }) {
  const stats = [
    { label: "Failed logins (24h)", value: data.failedLogins24h },
    { label: "Blocked IPs", value: data.blockedIps },
    { label: "Active sessions", value: data.activeSessions.toLocaleString() },
    { label: "2FA adoption", value: `${data.twoFactorAdoptionPct}%` },
  ];

  return (
    <div className="space-y-5">
      <div className="flex items-center gap-4 rounded-xl border border-border bg-background/40 p-4">
        <div className="relative flex h-16 w-16 items-center justify-center">
          <ShieldCheck className={cn("h-8 w-8", scoreColor(data.score))} />
        </div>
        <div>
          <div className="flex items-baseline gap-1">
            <span className={cn("text-2xl font-semibold tabular-nums", scoreColor(data.score))}>{data.score}</span>
            <span className="text-sm text-muted-foreground">/ 100</span>
          </div>
          <p className="text-xs text-muted-foreground">Security posture score</p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-2">
        {stats.map((stat) => (
          <div key={stat.label} className="rounded-lg border border-border bg-background/40 px-3 py-2">
            <p className="text-lg font-semibold tabular-nums">{stat.value}</p>
            <p className="text-[11px] text-muted-foreground">{stat.label}</p>
          </div>
        ))}
      </div>

      <ul className="space-y-2">
        {data.events.map((event) => {
          const meta = severityMeta[event.severity];
          const Icon = meta.icon;
          return (
            <li key={event.id} className="flex gap-2.5">
              <span className={cn("flex h-7 w-7 shrink-0 items-center justify-center rounded-lg", meta.className)}>
                <Icon className="h-3.5 w-3.5" />
              </span>
              <div className="min-w-0 flex-1">
                <p className="text-xs font-medium leading-tight">{event.label}</p>
                <p className="truncate text-[11px] text-muted-foreground">{event.detail}</p>
              </div>
              <span className="shrink-0 text-[11px] text-muted-foreground">{timeAgo(event.timestamp)}</span>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
