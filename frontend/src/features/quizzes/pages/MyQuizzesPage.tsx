import { useNavigate, useParams, Link } from "react-router-dom";
import { ArrowLeft, Clock, ListChecks, Repeat } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useQuizzes } from "../api/queries";
import { formatAttempts, formatTimeLimit, learnerQuizStatus } from "../lib/quizzes";

/** Learner view: the published quizzes on one course, with their own standing. */
export function MyQuizzesPage() {
  const { courseId = "" } = useParams();
  const navigate = useNavigate();
  const { data: quizzes, isLoading, isError, error } = useQuizzes(courseId);

  // The attempt page starts or resumes the sitting itself, so a refresh mid-quiz still works.
  const start = (quizId: string) => navigate(`/my-courses/${courseId}/quizzes/${quizId}/attempt`);

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-4xl px-6 py-10">
        <Link
          to="/my-courses"
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to my courses
        </Link>

        <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
          <ListChecks className="h-6 w-6 text-primary" aria-hidden />
          Quizzes
        </h1>
        <p className="mt-1 text-muted-foreground">Marked as soon as you hand them in.</p>

        {isError && (
          <Alert variant="error" className="mt-6">
            {getApiErrorMessage(error, "We could not load your quizzes.")}
          </Alert>
        )}

        {isLoading && (
          <div className="mt-8 space-y-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-28 rounded-[18px]" />
            ))}
          </div>
        )}

        {quizzes && quizzes.length === 0 && (
          <div className="mt-8 rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">No quizzes yet.</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Your lecturer has not published any for this course.
            </p>
          </div>
        )}

        <ul className="mt-8 space-y-3">
          {quizzes?.map((quiz) => {
            const status = learnerQuizStatus(quiz);

            return (
              <li key={quiz.id} className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <h2 className="truncate font-semibold">{quiz.title}</h2>
                      <Badge variant={status.variant}>{status.label}</Badge>
                    </div>
                    <p className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
                      <span>
                        {quiz.questionCount} question{quiz.questionCount === 1 ? "" : "s"} ·{" "}
                        {quiz.totalPoints} points
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <Clock className="h-3.5 w-3.5" aria-hidden />
                        {formatTimeLimit(quiz.timeLimitMinutes)}
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <Repeat className="h-3.5 w-3.5" aria-hidden />
                        {quiz.maxAttempts === null
                          ? formatAttempts(null)
                          : `${quiz.attemptsUsed} of ${quiz.maxAttempts} used`}
                      </span>
                      {quiz.passingScorePercent !== null && (
                        <span>Pass at {quiz.passingScorePercent}%</span>
                      )}
                    </p>
                  </div>

                  <Button
                    size="sm"
                    onClick={() => start(quiz.id)}
                    disabled={!quiz.canAttempt}
                    title={quiz.canAttempt ? undefined : "You have used all your attempts"}
                  >
                    {quiz.attemptsUsed === 0 ? "Start" : "Try again"}
                  </Button>
                </div>

                {quiz.description && (
                  <p className="mt-3 whitespace-pre-wrap text-sm text-muted-foreground">
                    {quiz.description}
                  </p>
                )}
              </li>
            );
          })}
        </ul>
      </main>
    </div>
  );
}
