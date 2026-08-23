import { History } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { timeAgo } from "@/lib/format";
import type { ReportRun } from "../api/types";

const reportTypeLabel: Record<ReportRun["type"], string> = {
  Enrollments: "Enrollments",
  Revenue: "Revenue",
  CoursePerformance: "Course performance",
  Users: "Users",
  SupportTickets: "Support tickets",
};

interface RecentReportRunsPanelProps {
  runs: ReportRun[] | undefined;
  isLoading: boolean;
}

/** Who has been generating which reports, most recent first — the audit trail every run leaves. */
export function RecentReportRunsPanel({ runs, isLoading }: RecentReportRunsPanelProps) {
  return (
    <div className="rounded-[18px] border border-border bg-card p-5 shadow-soft">
      <h3 className="flex items-center gap-1.5 text-sm font-semibold tracking-tight">
        <History className="h-4 w-4 text-primary" aria-hidden />
        Recent report runs
      </h3>

      {isLoading ? (
        <div className="mt-3 space-y-2">
          {Array.from({ length: 4 }).map((_, index) => (
            <Skeleton key={index} className="h-10 rounded-lg" />
          ))}
        </div>
      ) : !runs || runs.length === 0 ? (
        <p className="mt-3 text-sm text-muted-foreground">No reports have been run yet.</p>
      ) : (
        <ul className="mt-3 space-y-2">
          {runs.map((run) => (
            <li
              key={run.id}
              className="flex items-start justify-between gap-3 rounded-lg border border-border px-3 py-2 text-sm"
            >
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <Badge variant="outline">{reportTypeLabel[run.type]}</Badge>
                  <span className="truncate text-muted-foreground">{run.generatedByName}</span>
                </div>
                {run.filtersSummary && (
                  <p className="mt-1 truncate text-xs text-muted-foreground">{run.filtersSummary}</p>
                )}
              </div>
              <div className="shrink-0 text-right">
                <p className="tabular-nums">{run.rowCount} rows</p>
                <p className="text-xs text-muted-foreground">{timeAgo(run.createdAtUtc)}</p>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
