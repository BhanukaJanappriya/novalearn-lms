import { useEffect, useState } from "react";
import { CircleCheck, PenLine } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { Progress } from "@/components/ui/progress";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import { useAttemptResult, useMarkEssay } from "../api/queries";
import { attemptStatusLabels, attemptStatusVariant } from "../lib/quizzes";

interface EssayMarkingDialogProps {
  attemptId: string | null;
  open: boolean;
  onClose: () => void;
}

interface Draft {
  points: string;
  feedback: string;
}

/**
 * Marks the written answers in one attempt. Auto-marked questions are shown read only for
 * context, since a marker usually wants to see how the learner did on the rest.
 */
export function EssayMarkingDialog({ attemptId, open, onClose }: EssayMarkingDialogProps) {
  const { data: attempt, isLoading } = useAttemptResult(open ? attemptId ?? undefined : undefined);
  const markEssay = useMarkEssay();

  const [drafts, setDrafts] = useState<Record<string, Draft>>({});

  // Seed each essay's inputs from whatever mark it already carries.
  useEffect(() => {
    if (!attempt) return;
    setDrafts(
      Object.fromEntries(
        attempt.answers
          .filter((a) => a.requiresManualMarking)
          .map((a) => [
            a.answerId,
            { points: a.isManuallyMarked ? String(a.pointsAwarded) : "", feedback: a.feedback ?? "" },
          ]),
      ),
    );
  }, [attempt]);

  if (!attemptId) return null;

  const save = (answerId: string) => {
    const draft = drafts[answerId];
    if (!draft || !attempt) return;

    markEssay.mutate({
      attemptId: attempt.attemptId,
      answerId,
      pointsAwarded: Number(draft.points),
      feedback: draft.feedback.trim() || null,
    });
  };

  const essays = attempt?.answers.filter((a) => a.requiresManualMarking) ?? [];
  const autoMarked = attempt?.answers.filter((a) => !a.requiresManualMarking) ?? [];

  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Mark written answers"
      description={attempt ? `${attempt.studentName} · attempt ${attempt.attemptNumber}` : undefined}
      className="max-w-3xl"
    >
      <div className="space-y-4">
        {markEssay.isError ? <Alert variant="error">{getApiErrorMessage(markEssay.error)}</Alert> : null}

        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 2 }).map((_, i) => (
              <Skeleton key={i} className="h-40 rounded-xl" />
            ))}
          </div>
        ) : attempt ? (
          <>
            <div className="rounded-xl border border-border p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="text-sm text-muted-foreground">
                  {attempt.isAwaitingMarking ? "Provisional score" : "Final score"}
                </span>
                <Badge variant={attemptStatusVariant[attempt.status]}>
                  {attemptStatusLabels[attempt.status]}
                </Badge>
              </div>
              <div className="mt-2 flex items-baseline gap-3">
                <span className="text-3xl font-semibold tabular-nums">{attempt.scorePercent}%</span>
                <span className="text-sm text-muted-foreground">
                  {attempt.pointsAwarded} of {attempt.totalPoints}
                </span>
              </div>
              <Progress className="mt-3" value={attempt.scorePercent} label="Attempt score" />
              {attempt.isAwaitingMarking && (
                <p className="mt-2 text-xs text-muted-foreground">
                  {attempt.awaitingMarkingCount} written answer
                  {attempt.awaitingMarkingCount === 1 ? "" : "s"} still to mark. The score can only
                  go up.
                </p>
              )}
            </div>

            {essays.length === 0 ? (
              <p className="py-6 text-center text-sm text-muted-foreground">
                This attempt has no written answers to mark.
              </p>
            ) : (
              <ul className="space-y-3">
                {essays.map((answer) => {
                  const draft = drafts[answer.answerId] ?? { points: "", feedback: "" };
                  const points = Number(draft.points);
                  const invalid =
                    draft.points === "" ||
                    Number.isNaN(points) ||
                    points < 0 ||
                    points > answer.pointsPossible;

                  return (
                    <li key={answer.answerId} className="rounded-xl border border-border p-4">
                      <div className="flex flex-wrap items-start justify-between gap-2">
                        <p className="min-w-0 font-medium">{answer.questionText}</p>
                        <Badge variant={answer.isManuallyMarked ? "success" : "warning"}>
                          {answer.isManuallyMarked ? (
                            <>
                              <CircleCheck className="h-3 w-3" aria-hidden />
                              Marked
                            </>
                          ) : (
                            <>
                              <PenLine className="h-3 w-3" aria-hidden />
                              To mark
                            </>
                          )}
                        </Badge>
                      </div>

                      <p className="mt-3 whitespace-pre-wrap break-words rounded-lg bg-muted/50 p-3 text-sm">
                        {answer.textAnswer ?? "No answer given."}
                      </p>

                      <div className="mt-3 flex flex-wrap items-end gap-2">
                        <div className="w-32 space-y-1">
                          <label
                            htmlFor={`essay-points-${answer.answerId}`}
                            className="text-xs font-medium text-muted-foreground"
                          >
                            Points (of {answer.pointsPossible})
                          </label>
                          <Input
                            id={`essay-points-${answer.answerId}`}
                            type="number"
                            min={0}
                            max={answer.pointsPossible}
                            value={draft.points}
                            onChange={(e) =>
                              setDrafts({
                                ...drafts,
                                [answer.answerId]: { ...draft, points: e.target.value },
                              })
                            }
                          />
                        </div>
                        <div className="min-w-[200px] flex-1 space-y-1">
                          <label
                            htmlFor={`essay-feedback-${answer.answerId}`}
                            className="text-xs font-medium text-muted-foreground"
                          >
                            Feedback
                          </label>
                          <Textarea
                            id={`essay-feedback-${answer.answerId}`}
                            rows={2}
                            value={draft.feedback}
                            onChange={(e) =>
                              setDrafts({
                                ...drafts,
                                [answer.answerId]: { ...draft, feedback: e.target.value },
                              })
                            }
                            placeholder="Optional"
                            maxLength={4000}
                          />
                        </div>
                        <Button
                          size="sm"
                          onClick={() => save(answer.answerId)}
                          disabled={invalid || markEssay.isPending}
                        >
                          {answer.isManuallyMarked ? "Update mark" : "Save mark"}
                        </Button>
                      </div>
                    </li>
                  );
                })}
              </ul>
            )}

            {autoMarked.length > 0 && (
              <details className="rounded-xl border border-border p-3">
                <summary className="cursor-pointer text-sm font-medium">
                  Auto-marked answers ({autoMarked.length})
                </summary>
                <ul className="mt-3 space-y-2">
                  {autoMarked.map((answer) => (
                    <li key={answer.answerId} className="flex items-start justify-between gap-3 text-sm">
                      <span className="min-w-0">
                        <span className="block truncate">{answer.questionText}</span>
                        <span className="block text-xs text-muted-foreground">
                          {answer.selectedOptionTexts.join(", ") || answer.textAnswer || "No answer"}
                        </span>
                      </span>
                      <span
                        className={`shrink-0 font-semibold tabular-nums ${
                          answer.isCorrect ? "text-success" : "text-destructive"
                        }`}
                      >
                        {answer.pointsAwarded}/{answer.pointsPossible}
                      </span>
                    </li>
                  ))}
                </ul>
              </details>
            )}
          </>
        ) : null}

        <div className="flex justify-end pt-2">
          <Button variant="outline" onClick={onClose}>
            Close
          </Button>
        </div>
      </div>
    </Modal>
  );
}
