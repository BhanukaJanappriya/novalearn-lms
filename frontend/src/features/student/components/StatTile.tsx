import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

export interface StatTileProps {
  label: string;
  value: string;
  hint?: string;
  icon: LucideIcon;
  /** Tailwind text colour for the icon, e.g. "text-primary". */
  accent?: string;
}

/**
 * One headline figure. Deliberately chart free: the learner dashboard is the landing page for
 * every student, so pulling Recharts in here would hand them the admin bundle on sign-in.
 */
export function StatTile({ label, value, hint, icon: Icon, accent = "text-primary" }: StatTileProps) {
  return (
    <div
      className="flex flex-col gap-3 rounded-[18px] border border-border bg-card p-5 shadow-soft"
    >
      <div className="flex items-start justify-between gap-3">
        <p className="min-w-0 truncate text-sm text-muted-foreground">{label}</p>
        <span className={cn("shrink-0 rounded-xl bg-muted p-2", accent)}>
          <Icon className="h-4.5 w-4.5" aria-hidden />
        </span>
      </div>
      <div>
        <p className="text-3xl font-semibold tabular-nums tracking-tight">{value}</p>
        {hint ? <p className="mt-1 truncate text-xs text-muted-foreground">{hint}</p> : null}
      </div>
    </div>
  );
}
