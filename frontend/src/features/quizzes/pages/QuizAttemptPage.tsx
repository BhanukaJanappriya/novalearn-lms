import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, ListChecks, TriangleAlert } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useSaveAnswer, useStartAttempt, useSubmitAttempt } from "../api/queries";
import type { AttemptInProgress } from "../api/types";
import { AttemptTimer } from "../components/AttemptTimer";

/** One question's working state. Option ids are a set so checkboxes and radios share a shape. */
interface AnswerDraft {
  optionIds: string[];
  text: string;
}

const emptyDraft: AnswerDraft = { optionIds: [], text: "" };

/**
 * Sitting a quiz. Starting is idempotent on the server, so mounting this page either begins a
 * new attempt or resumes the one already open, which makes a refresh mid-quiz safe.
 */
export function QuizAttemptPage() {
  const { courseId = "", quizId = "" } = useParams();
  const navigate = useNavigate();

  const startAttempt = useStartAttempt();
  const saveAnswer = useSaveAnswer();
  const submitAttempt = useSubmitAttempt();

  const [attempt, setAttempt] = useState<AttemptInProgress | null>(null);
  const [answers, setAnswers] = useState<Record<string, AnswerDraft>>({});
  const [confirmOpen, setConfirmOpen] = useState(false);

  // Guards the effect against StrictMode's double mount, which would otherwise fire two starts.
  const startedRef = useRef(false);
  const submittingRef = useRef(false);

  useEffect(() => {
    if (startedRef.current || !quizId) return;
    startedRef.current = true;

    startAttempt.mutate(quizId, {
      onSuccess: (data) => {
        setAttempt(data);
        setAnswers(
          Object.fromEntries(
            data.questions.map((q) => [
              q.id,
              { optionIds: q.selectedOptionIds, text: q.textAnswer ?? "" },
            ]),
          ),
        );
      },
    });
  }, [quizId, startAttempt]);

  const submit = useCallback(() => {
    if (!attempt || submittingRef.current) return;
    submittingRef.current = true;

    submitAttempt.mutate(attempt.attemptId, {
      onSuccess: (result) =>
        navigate(`/my-courses/${courseId}/quizzes/${quizId}/result/${result.attemptId}`, {
          replace: true,
        }),
      onError: () => {
        submittingRef.current = false;
      },
    });
  }, [attempt, courseId, navigate, quizId, submitAttempt]);

  const persist = (questionId: string, optionIds: string[], text: string) => {
    if (!attempt) return;
    saveAnswer.mutate({
      attemptId: attempt.attemptId,
      questionId,
      selectedOptionIds: optionIds,
      textAnswer: text.trim() || null,
    });
  };

  /** Radio behaviour: the chosen id replaces whatever was there. */
  const chooseOption = (questionId: string, optionId: string) => {
    setAnswers((current) => ({ ...current, [questionId]: { optionIds: [optionId], text: "" } }));
    persist(questionId, [optionId], "");
  };

  /** Checkbox behaviour: the id joins or leaves the set. */
  const toggleOption = (questionId: string, optionId: string) => {
    const current = answers[questionId]?.optionIds ?? [];
    const next = current.includes(optionId)
      ? current.filter((id) => id !== optionId)
      : [...current, optionId];

    setAnswers((state) => ({ ...state, [questionId]: { optionIds: next, text: "" } }));
    persist(questionId, next, "");
  };

  const typeAnswer = (questionId: string, text: string) =>
    setAnswers((current) => ({ ...current, [questionId]: { optionIds: [], text } }));

  const isAnswered = (questionId: string) => {
    const answer = answers[questionId];
    return Boolean(answer && (answer.optionIds.length > 0 || answer.text.trim().length > 0));
  };

  const missingRequired = attempt
    ? attempt.questions.filter((q) => q.isRequired && !isAnswered(q.id))
    : [];

  const answeredCount = attempt ? attempt.questions.filter((q) => isAnswered(q.id)).length : 0;

  const essayCount = attempt ? attempt.questions.filter((q) => q.isEssay).length : 0;

  if (startAttempt.isPending || (!attempt && !startAttempt.isError)) {
    return (
      <div className="min-h-screen">
        <LearnerHeader />
        <main className="mx-auto max-w-3xl px-6 py-10">
          <Skeleton className="h-10 w-64" />
          <div className="mt-8 space-y-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-40 rounded-[18px]" />
            ))}
          </div>
        </main>
      </div>
    );
  }

  if (startAttempt.isError) {
    return (
      <div className="min-h-screen">
        <LearnerHeader />
        <main className="mx-auto max-w-3xl px-6 py-10">
          <Alert variant="error">
            {getApiErrorMessage(startAttempt.error, "We could not start this quiz.")}
          </Alert>
          <Link
            to={`/my-courses/${courseId}/quizzes`}
            className="mt-4 inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" aria-hidden />
            Back to quizzes
          </Link>
        </main>
      </div>
    );
  }

  if (!attempt) return null;

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-3xl px-6 py-10">
        <header className="mb-6">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div className="min-w-0">
              <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
                <ListChecks className="h-6 w-6 text-primary" aria-hidden />
                {attempt.quizTitle}
              </h1>
              <p className="mt-1 text-sm text-muted-foreground">
                Attempt {attempt.attemptNumber} · {attempt.totalPoints} points
                {essayCount > 0 &&
                  ` · ${essayCount} written answer${essayCount === 1 ? "" : "s"} marked by your lecturer`}
              </p>
            </div>
            {attempt.deadlineUtc && (
              <AttemptTimer deadlineUtc={attempt.deadlineUtc} onExpire={submit} />
            )}
          </div>

          <div className="mt-4">
            <div className="mb-1.5 flex items-center justify-between text-xs text-muted-foreground">
              <span>
                {answeredCount} of {attempt.questions.length} answered
              </span>
            </div>
            <Progress
              value={(answeredCount / Math.max(1, attempt.questions.length)) * 100}
              label="Questions answered"
              size="sm"
            />
          </div>
        </header>

        {submitAttempt.isError && (
          <Alert variant="error" className="mb-4">
            {getApiErrorMessage(submitAttempt.error, "We could not submit your attempt.")}
          </Alert>
        )}

        <ol className="space-y-4">
          {attempt.questions.map((question, index) => {
            const answer = answers[question.id] ?? emptyDraft;

            return (
              <li
                key={question.id}
                className="rounded-[18px] border border-border bg-card p-5 shadow-soft"
              >
                <div className="flex items-start justify-between gap-3">
                  <p className="font-medium">
                    <span className="mr-2 text-sm text-muted-foreground">Q{index + 1}</span>
                    {question.text}
                    {question.isRequired && (
                      <span className="ml-1 text-destructive" aria-label="Required">
                        *
                      </span>
                    )}
                  </p>
                  <span className="flex shrink-0 flex-wrap items-center justify-end gap-1">
                    {question.isEssay && <Badge variant="warning">Marked by hand</Badge>}
                    <Badge variant="neutral">{question.points} pts</Badge>
                  </span>
                </div>

                {question.isEssay ? (
                  <>
                    <Textarea
                      className="mt-4"
                      rows={8}
                      value={answer.text}
                      onChange={(e) => typeAnswer(question.id, e.target.value)}
                      onBlur={() => persist(question.id, [], answer.text)}
                      placeholder="Write your answer"
                      aria-label={`Answer to question ${index + 1}`}
                      maxLength={20000}
                    />
                    <p className="mt-1.5 text-xs text-muted-foreground">
                      Your lecturer marks this one, so your score will not be final straight away.
                    </p>
                  </>
                ) : question.type === "ShortAnswer" ? (
                  <Input
                    className="mt-4"
                    value={answer.text}
                    onChange={(e) => typeAnswer(question.id, e.target.value)}
                    onBlur={() => persist(question.id, [], answer.text)}
                    placeholder="Type your answer"
                    aria-label={`Answer to question ${index + 1}`}
                    maxLength={2000}
                  />
                ) : (
                  <ul className="mt-4 space-y-2">
                    {question.options.map((option) => {
                      const selected = answer.optionIds.includes(option.id);

                      return (
                        <li key={option.id}>
                          <label
                            className={`flex cursor-pointer items-center gap-3 rounded-xl border p-3 transition-colors ${
                              selected
                                ? "border-primary bg-primary/5"
                                : "border-border hover:bg-muted/50"
                            }`}
                          >
                            <input
                              type={question.allowsMultipleSelections ? "checkbox" : "radio"}
                              name={question.allowsMultipleSelections ? undefined : question.id}
                              checked={selected}
                              onChange={() =>
                                question.allowsMultipleSelections
                                  ? toggleOption(question.id, option.id)
                                  : chooseOption(question.id, option.id)
                              }
                              className="h-4 w-4 shrink-0 accent-primary"
                            />
                            <span className="text-sm">{option.text}</span>
                          </label>
                        </li>
                      );
                    })}
                    {question.allowsMultipleSelections && (
                      <li className="pt-1 text-xs text-muted-foreground">
                        Select every answer that applies.
                      </li>
                    )}
                  </ul>
                )}
              </li>
            );
          })}
        </ol>

        <div className="mt-6 flex flex-wrap items-center justify-between gap-3">
          <p className="text-xs text-muted-foreground">
            {missingRequired.length > 0
              ? `${missingRequired.length} required question${
                  missingRequired.length === 1 ? "" : "s"
                } still to answer.`
              : "Answers save as you go. You can leave and come back."}
          </p>
          <Button
            onClick={() => setConfirmOpen(true)}
            isLoading={submitAttempt.isPending}
            disabled={missingRequired.length > 0}
          >
            Submit attempt
          </Button>
        </div>
      </main>

      <Modal
        open={confirmOpen}
        onClose={() => setConfirmOpen(false)}
        title="Submit this attempt?"
        description={attempt.quizTitle}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
            <p className="text-muted-foreground">
              {answeredCount < attempt.questions.length
                ? `You have answered ${answeredCount} of ${attempt.questions.length}. Unanswered questions score zero.`
                : "Your attempt cannot be changed once submitted."}
              {essayCount > 0 &&
                " Your written answers go to your lecturer, so the score you see first is provisional."}
            </p>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setConfirmOpen(false)}>
              Keep working
            </Button>
            <Button
              onClick={() => {
                setConfirmOpen(false);
                submit();
              }}
              isLoading={submitAttempt.isPending}
            >
              Submit
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
