import { cn } from "@/lib/utils";
import type { ServiceStatus, SystemHealth as SystemHealthData } from "../api/types";

const statusMeta: Record<ServiceStatus, { dot: string; label: string; text: string }> = {
  operational: { dot: "bg-success", label: "Operational", text: "text-success" },
  degraded: { dot: "bg-[hsl(var(--warning))]", label: "Degraded", text: "text-[hsl(var(--warning))]" },
  down: { dot: "bg-destructive", label: "Down", text: "text-destructive" },
};

function resourceColor(pct: number): string {
  if (pct >= 85) return "bg-destructive";
  if (pct >= 70) return "bg-[hsl(var(--warning))]";
  return "bg-success";
}

export function SystemHealth({ data }: { data: SystemHealthData }) {
  return (
    <div className="space-y-5">
      <div className="grid grid-cols-2 gap-2">
        {data.services.map((service) => {
          const meta = statusMeta[service.status];
          return (
            <div key={service.name} className="flex items-center gap-2 rounded-lg border border-border bg-background/40 px-3 py-2">
              <span className={cn("h-2 w-2 shrink-0 rounded-full", meta.dot, service.status !== "operational" && "animate-pulse")} />
              <div className="min-w-0">
                <p className="truncate text-xs font-medium">{service.name}</p>
                <p className={cn("text-[11px]", meta.text)}>{service.latencyMs}ms · {service.uptimePct}%</p>
              </div>
            </div>
          );
        })}
      </div>

      <div className="space-y-3">
        {data.resources.map((resource) => (
          <div key={resource.name}>
            <div className="mb-1 flex items-center justify-between text-xs">
              <span className="text-muted-foreground">{resource.name}</span>
              <span className="font-medium tabular-nums">{resource.usagePct}%</span>
            </div>
            <div className="h-1.5 overflow-hidden rounded-full bg-muted">
              <div
                className={cn("h-full rounded-full transition-all duration-700", resourceColor(resource.usagePct))}
                style={{ width: `${resource.usagePct}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
