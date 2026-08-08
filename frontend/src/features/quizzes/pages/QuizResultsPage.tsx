import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ArrowLeft, BarChart3, Clock, PenLine } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import { timeAgo } from "@/lib/format";
import { useQuizResults } from "../api/queries";
import { EssayMarkingDialog } from "../components/EssayMarkingDialog";
import { attemptStatusLabels, attemptStatusVariant } from "../lib/quizzes";

/** Lecturer view: how the cohort did, and the queue of attempts still needing a person. */
export function QuizResultsPage() {
  const { courseId = "", quizId = "" } = useParams();
  const { data, isLoading, isError, error } = useQuizResults(quizId);
  const [marking, setMarking] = useState<string | null>(null);

  return (
    <PageTransition>
      <div className="space-y-6">
      <header>
        <Link
          to={`/admin/courses/${courseId}/quizzes`}
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to quizzes
        </Link>
        <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
          <BarChart3 className="h-6 w-6 text-primary" aria-hidden />
          Quiz results
        </h1>
        {data && (
          <p className="mt-1 text-muted-foreground">
            {data.quizTitle} · {data.totalPoints} points
            {data.passingScorePercent !== null && ` · pass at ${data.passingScorePercent}%`}
          </p>
        )}
      </header>

      {isError && (
        <Alert variant="error">{getApiErrorMessage(error, "We could not load the results.")}</Alert>
      )}

      {isLoading && <Skeleton className="h-64 rounded-[18px]" />}

      {data && (
        <>
          {data.awaitingReviewCount > 0 && (
            <div className="flex items-start gap-3 rounded-xl border border-[hsl(var(--warning))]/30 bg-warning/5 p-3 text-sm">
              <PenLine className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
              <p className="text-muted-foreground">
                {data.awaitingReviewCount} attempt{data.awaitingReviewCount === 1 ? "" : "s"} contain
                written answers waiting on you. Their scores stay provisional until you mark them.
              </p>
            </div>
          )}

          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <Tile label="Attempts" value={String(data.attemptCount)} />
            <Tile label="Learners" value={String(data.distinctLearners)} />
            <Tile
              label="Awaiting marking"
              value={String(data.awaitingReviewCount)}
              accent={data.awaitingReviewCount > 0 ? "text-[hsl(var(--warning))]" : undefined}
            />
            <Tile
              label="Average score"
              value={data.averageScorePercent === null ? "—" : `${data.averageScorePercent}%`}
              hint={data.awaitingReviewCount > 0 ? "Includes provisional scores" : undefined}
            />
          </div>

          {data.attempts.length === 0 ? (
            <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
              <p className="font-medium">Nobody has sat this quiz yet.</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Results appear here as learners submit.
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto rounded-[18px] border border-border">
              <table className="w-full min-w-[760px] border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border bg-muted/40 text-left">
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Learner</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Attempt</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Score</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Outcome</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Submitted</th>
                    <th scope="col" className="px-4 py-3 text-right font-medium text-muted-foreground">
                      Marking
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {data.attempts.map((attempt) => (
                    <tr
                      key={attempt.attemptId}
                      className="border-b border-border last:border-0 hover:bg-muted/30"
                    >
                      <td className="px-4 py-3">
                        <span className="block truncate font-medium">{attempt.studentName}</span>
                        <span className="block truncate text-xs text-muted-foreground">
                          {attempt.studentEmail}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">#{attempt.attemptNumber}</td>
                      <td className="px-4 py-3">
                        <span className="font-semibold tabular-nums">{attempt.scorePercent}%</span>
                        <span className="block text-xs text-muted-foreground">
                          {attempt.pointsAwarded}/{attempt.totalPoints}
                          {attempt.isAwaitingMarking && " so far"}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex flex-wrap gap-1">
                          <Badge variant={attemptStatusVariant[attempt.status]}>
                            {attemptStatusLabels[attempt.status]}
                          </Badge>
                          {!attempt.isAwaitingMarking && data.passingScorePercent !== null && (
                            <Badge variant={attempt.isPassed ? "success" : "warning"}>
                              {attempt.isPassed ? "Passed" : "Not passed"}
                            </Badge>
                          )}
                          {attempt.wasLate && (
                            <Badge variant="destructive">
                              <Clock className="h-3 w-3" aria-hidden />
                              Late
                            </Badge>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-xs text-muted-foreground">
                        {attempt.submittedAtUtc ? timeAgo(attempt.submittedAtUtc) : "—"}
                      </td>
                      <td className="px-4 py-3 text-right">
                        {attempt.isAwaitingMarking ? (
                          <Button size="sm" onClick={() => setMarking(attempt.attemptId)}>
                            <PenLine className="h-3.5 w-3.5" />
                            Mark {attempt.awaitingMarkingCount}
                          </Button>
                        ) : (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => setMarking(attempt.attemptId)}
                          >
                            Review
                          </Button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <EssayMarkingDialog
        attemptId={marking}
        open={marking !== null}
        onClose={() => setMarking(null)}
      />
      </div>
    </PageTransition>
  );
}

function Tile({ label, value, hint, accent }: { label: string; value: string; hint?: string; accent?: string }) {
  return (
    <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className={`mt-1 text-2xl font-semibold tabular-nums ${accent ?? ""}`}>{value}</p>
      {hint && <p className="mt-0.5 text-xs text-muted-foreground">{hint}</p>}
    </div>
  );
}
