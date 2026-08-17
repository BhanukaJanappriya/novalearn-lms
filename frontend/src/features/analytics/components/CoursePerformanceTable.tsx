import { Link } from "react-router-dom";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { CoursePerformanceRow } from "../api/queries";

interface CoursePerformanceTableProps {
  rows: CoursePerformanceRow[];
}

/**
 * Per course figures, busiest first.
 *
 * These are lifetime figures rather than window ones, which the caption says out loud. A
 * completion rate only means anything against everyone who ever enrolled: measured over the last
 * month it would compare this month's finishers with this month's joiners and swing for reasons
 * nobody could act on.
 */
export function CoursePerformanceTable({ rows }: CoursePerformanceTableProps) {
  if (rows.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No courses yet.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Course</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Enrolled</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Completion</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Avg progress</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Avg mark</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">At risk</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.courseId} className="border-b border-border last:border-0">
              <td className="py-3 pr-3">
                <Link
                  to={`/admin/courses/${row.courseId}/gradebook`}
                  className="font-medium hover:underline"
                >
                  {row.title}
                </Link>
                <span className="block text-xs text-muted-foreground">
                  {row.departmentName ?? "No department"}
                </span>
              </td>

              <td className="py-3 text-right tabular-nums">{row.enrolled}</td>

              <td className="py-3 pr-3">
                <div className="flex items-center gap-2">
                  <Progress
                    value={row.completionRatePercent}
                    label={`${row.title} completion`}
                    size="sm"
                    className="min-w-[70px] flex-1"
                  />
                  <span className="w-11 text-right text-xs tabular-nums text-muted-foreground">
                    {row.completionRatePercent}%
                  </span>
                </div>
              </td>

              <td className="py-3 pr-3">
                <div className="flex items-center gap-2">
                  <Progress
                    value={row.averageProgressPercent}
                    label={`${row.title} average progress`}
                    size="sm"
                    className="min-w-[70px] flex-1"
                  />
                  <span className="w-11 text-right text-xs tabular-nums text-muted-foreground">
                    {row.averageProgressPercent}%
                  </span>
                </div>
              </td>

              <td className="py-3 text-right tabular-nums">
                {row.averageScorePercent === null ? (
                  <span className="text-muted-foreground">Nothing marked</span>
                ) : (
                  `${row.averageScorePercent}%`
                )}
              </td>

              <td className="py-3 text-right">
                {row.atRiskCount > 0 ? (
                  <Badge variant="warning">{row.atRiskCount}</Badge>
                ) : (
                  <span className="text-muted-foreground">0</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
