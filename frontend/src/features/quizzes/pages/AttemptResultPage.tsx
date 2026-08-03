import { Link, useParams } from "react-router-dom";
import { ArrowLeft, CircleCheck, CircleX, Clock } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { LinkButton } from "@/components/ui/link-button";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useAttemptResult } from "../api/queries";
import { questionTypeLabels } from "../lib/quizzes";

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
                <div className="flex items-center gap-2">
                  {data.wasLate && (
                    <Badge variant="destructive">
                      <Clock className="h-3 w-3" aria-hidden />
                      Late
                    </Badge>
                  )}
                  {data.passingScorePercent !== null && (
                    <Badge variant={data.isPassed ? "success" : "warning"}>
                      {data.isPassed ? "Passed" : "Not passed"}
                    </Badge>
                  )}
                </div>
              </div>

              <div className="mt-5">
                <div className="flex items-baseline gap-3">
                  <span className="text-4xl font-semibold tabular-nums">{data.scorePercent}%</span>
                  <span className="text-sm text-muted-foreground">
                    {data.pointsAwarded} of {data.totalPoints} points
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
              {data.answers.map((answer, index) => (
                <li
                  key={answer.questionId}
                  className={`rounded-[18px] border p-4 shadow-soft ${
                    answer.isCorrect
                      ? "border-[hsl(var(--success))]/30 bg-success/5"
                      : "border-destructive/30 bg-destructive/5"
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <p className="font-medium">
                      <span className="mr-2 text-sm text-muted-foreground">Q{index + 1}</span>
                      {answer.questionText}
                    </p>
                    <span className="flex shrink-0 items-center gap-1.5 text-sm font-semibold">
                      {answer.isCorrect ? (
                        <CircleCheck className="h-4 w-4 text-success" aria-hidden />
                      ) : (
                        <CircleX className="h-4 w-4 text-destructive" aria-hidden />
                      )}
                      {answer.pointsAwarded}/{answer.pointsPossible}
                    </span>
                  </div>

                  <p className="mt-2 text-xs text-muted-foreground">
                    {questionTypeLabels[answer.questionType]}
                  </p>

                  <dl className="mt-3 space-y-1.5 text-sm">
                    <div className="flex gap-2">
                      <dt className="shrink-0 text-muted-foreground">You answered:</dt>
                      <dd className={answer.isCorrect ? "text-success" : "text-destructive"}>
                        {answer.selectedOptionText ?? answer.textAnswer ?? "Nothing"}
                      </dd>
                    </div>
                    {!answer.isCorrect && (
                      <div className="flex gap-2">
                        <dt className="shrink-0 text-muted-foreground">Correct answer:</dt>
                        <dd className="text-success">
                          {answer.correctOptionText ?? answer.acceptedAnswers.join(" or ") ?? "—"}
                        </dd>
                      </div>
                    )}
                  </dl>
                </li>
              ))}
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
