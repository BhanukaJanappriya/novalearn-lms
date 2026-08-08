import { Link, useParams } from "react-router-dom";
import { ArrowLeft, ClipboardList, Table2 } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { LinkButton } from "@/components/ui/link-button";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import { useGradebook } from "../api/queries";
import { cellVariant } from "../lib/assessments";

/** Lecturer view: every enrolled learner against every published assignment. */
export function GradebookPage() {
  const { courseId = "" } = useParams();
  const { data, isLoading, isError, error } = useGradebook(courseId);

  return (
    <PageTransition>
      <div className="space-y-6">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <Link
            to={`/admin/courses/${courseId}/assignments`}
            className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" aria-hidden />
            Back to assignments
          </Link>
          <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <Table2 className="h-6 w-6 text-primary" aria-hidden />
            Gradebook
          </h1>
          {data && (
            <p className="mt-1 text-muted-foreground">
              {data.courseTitle} · {data.courseCode} · {data.totalPointsAvailable} points available
            </p>
          )}
        </div>
        <LinkButton to={`/admin/courses/${courseId}/assignments`} variant="outline" size="sm">
          <ClipboardList className="h-4 w-4" />
          Assignments
        </LinkButton>
      </header>

      {isError && (
        <Alert variant="error">{getApiErrorMessage(error, "We could not load the gradebook.")}</Alert>
      )}

      {isLoading && <Skeleton className="h-72 rounded-[18px]" />}

      {data && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <SummaryTile label="Learners" value={String(data.summary.studentCount)} />
            <SummaryTile label="Assignments" value={String(data.summary.assignmentCount)} />
            <SummaryTile
              label="Awaiting marking"
              value={String(data.summary.awaitingMarking)}
              accent={data.summary.awaitingMarking > 0 ? "text-[hsl(var(--warning))]" : undefined}
            />
            <SummaryTile
              label="Cohort average"
              value={
                data.summary.cohortAveragePercentage === null
                  ? "—"
                  : `${data.summary.cohortAveragePercentage}%`
              }
            />
          </div>

          {data.rows.length === 0 || data.assignments.length === 0 ? (
            <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
              <p className="font-medium">
                {data.assignments.length === 0
                  ? "No published assignments yet."
                  : "Nobody is enrolled on this course yet."}
              </p>
              <p className="mt-1 text-sm text-muted-foreground">
                {data.assignments.length === 0
                  ? "Publish an assignment and it will appear as a column here."
                  : "Learners appear as rows once they enrol."}
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto rounded-[18px] border border-border">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border bg-muted/40 text-left">
                    <th scope="col" className="sticky left-0 bg-muted/40 px-4 py-3 font-medium text-muted-foreground">
                      Learner
                    </th>
                    {data.assignments.map((column) => (
                      <th
                        key={column.assignmentId}
                        scope="col"
                        className="min-w-[130px] px-4 py-3 font-medium text-muted-foreground"
                      >
                        <span className="block truncate" title={column.title}>
                          {column.title}
                        </span>
                        <span className="block text-xs font-normal">
                          out of {column.maxPoints}
                          {column.averagePoints !== null && ` · avg ${column.averagePoints}`}
                        </span>
                      </th>
                    ))}
                    <th scope="col" className="px-4 py-3 text-right font-medium text-muted-foreground">
                      Total
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.rows.map((row) => (
                    <tr key={row.studentId} className="border-b border-border last:border-0 hover:bg-muted/30">
                      <td className="sticky left-0 bg-card px-4 py-3">
                        <span className="block truncate font-medium">{row.studentName}</span>
                        <span className="block truncate text-xs text-muted-foreground">
                          {row.studentEmail}
                        </span>
                      </td>
                      {row.cells.map((cell) => (
                        <td key={cell.assignmentId} className="px-4 py-3">
                          {cell.status === "Graded" ? (
                            <span className="font-semibold tabular-nums">{cell.pointsAwarded}</span>
                          ) : (
                            <Badge variant={cellVariant[cell.status]}>{cell.status}</Badge>
                          )}
                          {cell.isLate && (
                            <span className="ml-1.5 text-xs text-destructive" title="Handed in late">
                              late
                            </span>
                          )}
                        </td>
                      ))}
                      <td className="px-4 py-3 text-right">
                        <span className="font-semibold tabular-nums">
                          {row.percentageOfGraded === null ? "—" : `${row.percentageOfGraded}%`}
                        </span>
                        <span className="block text-xs text-muted-foreground">
                          {row.pointsAwarded}/{row.pointsGraded} marked
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <p className="text-xs text-muted-foreground">
            Percentages count marked work only, so outstanding submissions do not drag a learner
            down before they have been assessed.
          </p>
        </>
      )}
      </div>
    </PageTransition>
  );
}

function SummaryTile({ label, value, accent }: { label: string; value: string; accent?: string }) {
  return (
    <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className={`mt-1 text-2xl font-semibold tabular-nums ${accent ?? ""}`}>{value}</p>
    </div>
  );
}
