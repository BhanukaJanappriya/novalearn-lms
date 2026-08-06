import { Link, useParams } from "react-router-dom";
import { ArrowLeft, CircleCheck, CircleX, Clock, PenLine } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { LinkButton } from "@/components/ui/link-button";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useAttemptResult } from "../api/queries";
import type { AnswerResult } from "../api/types";
import { attemptStatusLabels, attemptStatusVariant, questionTypeLabels } from "../lib/quizzes";

/** What the learner should see as the "right answer" line for a marked question. */
function correctAnswerText(answer: AnswerResult): string {
  if (answer.correctOptionTexts.length > 0) return answer.correctOptionTexts.join(", ");
  if (answer.acceptedAnswers.length > 0) return answer.acceptedAnswers.join(" or ");
  return "—";
}

/** A marked attempt. Correct answers appear here because the attempt is closed. */
export function AttemptResultPage() {
  const { courseId = "", attemptId = "" } = useParams();
  const { data, isLoading, isError, error } = useAttemptResult(attemptId);

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-3xl px-6 py-10">
        <Link
          to={`/my-courses/${courseId}/quizzes`}
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to quizzes
        </Link>

        {isError && (
          <Alert variant="error" className="mt-6">
            {getApiErrorMessage(error, "We could not load this result.")}
          </Alert>
        )}

        {isLoading && (
          <div className="mt-6 space-y-3">
            <Skeleton className="h-32 rounded-[18px]" />
            <Skeleton className="h-40 rounded-[18px]" />
          </div>
        )}

        {data && (
          <>
            <section className="mt-4 rounded-[18px] border border-border bg-card p-6 shadow-soft">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h1 className="text-2xl font-semibold tracking-tight">{data.quizTitle}</h1>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Attempt {data.attemptNumber}
                    {data.wasLate && " · handed in after the time limit"}
                  </p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  {data.wasLate && (
                    <Badge variant="destructive">
                      <Clock className="h-3 w-3" aria-hidden />
                      Late
                    </Badge>
                  )}
                  <Badge variant={attemptStatusVariant[data.status]}>
                    {attemptStatusLabels[data.status]}
                  </Badge>
                  {!data.isAwaitingMarking && data.passingScorePercent !== null && (
                    <Badge variant={data.isPassed ? "success" : "warning"}>
                      {data.isPassed ? "Passed" : "Not passed"}
                    </Badge>
                  )}
                </div>
              </div>

              {data.isAwaitingMarking && (
                <div className="mt-4 flex items-start gap-3 rounded-xl border border-[hsl(var(--warning))]/30 bg-warning/5 p-3 text-sm">
                  <PenLine className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
                  <p className="text-muted-foreground">
                    {data.awaitingMarkingCount} written answer
                    {data.awaitingMarkingCount === 1 ? " is" : "s are"} still with your lecturer.
                    The score below counts only what was marked automatically, so it can only go up.
                  </p>
                </div>
              )}

              <div className="mt-5">
                <div className="flex items-baseline gap-3">
                  <span className="text-4xl font-semibold tabular-nums">{data.scorePercent}%</span>
                  <span className="text-sm text-muted-foreground">
                    {data.pointsAwarded} of {data.totalPoints} points
                    {data.isAwaitingMarking && " so far"}
                  </span>
                </div>
                <Progress
                  className="mt-3"
                  value={data.scorePercent}
                  label={data.quizTitle}
                  indicatorClassName={data.isPassed ? "bg-success" : undefined}
                />
                {data.passingScorePercent !== null && (
                  <p className="mt-2 text-xs text-muted-foreground">
                    Pass mark {data.passingScorePercent}%
                  </p>
                )}
              </div>
            </section>

            <h2 className="mt-8 text-lg font-semibold tracking-tight">Your answers</h2>

            <ol className="mt-3 space-y-3">
              {data.answers.map((answer, index) => {
                const pending = answer.isAwaitingMarking;
                const tone = pending
                  ? "border-[hsl(var(--warning))]/30 bg-warning/5"
                  : answer.isCorrect
                    ? "border-[hsl(var(--success))]/30 bg-success/5"
                    : "border-destructive/30 bg-destructive/5";

                return (
                  <li key={answer.answerId} className={`rounded-[18px] border p-4 shadow-soft ${tone}`}>
                    <div className="flex items-start justify-between gap-3">
                      <p className="font-medium">
                        <span className="mr-2 text-sm text-muted-foreground">Q{index + 1}</span>
                        {answer.questionText}
                      </p>
                      <span className="flex shrink-0 items-center gap-1.5 text-sm font-semibold">
                        {pending ? (
                          <PenLine className="h-4 w-4 text-[hsl(var(--warning))]" aria-hidden />
                        ) : answer.isCorrect ? (
                          <CircleCheck className="h-4 w-4 text-success" aria-hidden />
                        ) : (
                          <CircleX className="h-4 w-4 text-destructive" aria-hidden />
                        )}
                        {pending ? `—/${answer.pointsPossible}` : `${answer.pointsAwarded}/${answer.pointsPossible}`}
                      </span>
                    </div>

                    <p className="mt-2 text-xs text-muted-foreground">
                      {questionTypeLabels[answer.questionType]}
                    </p>

                    <dl className="mt-3 space-y-1.5 text-sm">
                      <div className="flex gap-2">
                        <dt className="shrink-0 text-muted-foreground">You answered:</dt>
                        <dd
                          className={
                            pending
                              ? "whitespace-pre-wrap"
                              : answer.isCorrect
                                ? "text-success"
                                : "text-destructive"
                          }
                        >
                          {answer.selectedOptionTexts.length > 0
                            ? answer.selectedOptionTexts.join(", ")
                            : answer.textAnswer ?? "Nothing"}
                        </dd>
                      </div>

                      {pending && (
                        <p className="text-xs text-muted-foreground">
                          Waiting on your lecturer to mark this.
                        </p>
                      )}

                      {!pending && !answer.requiresManualMarking && answer.isCorrect === false && (
                        <div className="flex gap-2">
                          <dt className="shrink-0 text-muted-foreground">Correct answer:</dt>
                          <dd className="text-success">{correctAnswerText(answer)}</dd>
                        </div>
                      )}

                      {answer.feedback && (
                        <div className="mt-2 rounded-lg bg-muted/60 p-2.5">
                          <dt className="text-xs font-medium text-muted-foreground">Feedback</dt>
                          <dd className="mt-0.5 whitespace-pre-wrap text-sm">{answer.feedback}</dd>
                        </div>
                      )}
                    </dl>
                  </li>
                );
              })}
            </ol>

            <div className="mt-6">
              <LinkButton to={`/my-courses/${courseId}/quizzes`} variant="outline" size="sm">
                <ArrowLeft className="h-4 w-4" />
                Back to quizzes
              </LinkButton>
            </div>
          </>
        )}
      </main>
    </div>
  );
}
