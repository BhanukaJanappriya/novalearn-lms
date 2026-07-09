import { ArrowRight, Lightbulb, Sparkles, TrendingUp, TriangleAlert } from "lucide-react";
import { cn } from "@/lib/utils";
import type { AiInsight, InsightSeverity } from "../api/types";

const severityMeta: Record<
  InsightSeverity,
  { icon: typeof Lightbulb; ring: string; chip: string; label: string }
> = {
  opportunity: { icon: TrendingUp, ring: "border-success/30", chip: "bg-success/10 text-success", label: "Opportunity" },
  attention: { icon: Lightbulb, ring: "border-warning/30", chip: "bg-warning/10 text-[hsl(var(--warning))]", label: "Attention" },
  risk: { icon: TriangleAlert, ring: "border-destructive/30", chip: "bg-destructive/10 text-destructive", label: "Risk" },
};

export function AiInsightsPanel({ items }: { items: AiInsight[] }) {
  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <Sparkles className="h-3.5 w-3.5 text-primary" />
        Generated from platform activity · updated hourly
      </div>
      {items.map((insight) => {
        const meta = severityMeta[insight.severity];
        const Icon = meta.icon;
        return (
          <div
            key={insight.id}
            className={cn("rounded-xl border bg-background/40 p-3.5 transition-colors hover:bg-background/70", meta.ring)}
          >
            <div className="mb-1.5 flex items-center gap-2">
              <span className={cn("inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[11px] font-medium", meta.chip)}>
                <Icon className="h-3 w-3" />
                {meta.label}
              </span>
              {insight.metric && (
                <span className="text-xs font-semibold text-foreground">{insight.metric}</span>
              )}
            </div>
            <p className="text-sm font-medium leading-snug">{insight.title}</p>
            <p className="mt-1 text-xs text-muted-foreground">{insight.body}</p>
            <button
              type="button"
              className="mt-2 inline-flex items-center gap-1 text-xs font-medium text-primary hover:underline"
            >
              {insight.actionLabel}
              <ArrowRight className="h-3 w-3" />
            </button>
          </div>
        );
      })}
    </div>
  );
}
