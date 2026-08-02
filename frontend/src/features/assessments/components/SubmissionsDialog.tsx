import { useEffect, useState } from "react";
import { Clock, ExternalLink, Inbox } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import { timeAgo } from "@/lib/format";
import { useGradeSubmission, useSubmissions } from "../api/queries";
import type { Assignment } from "../api/types";
import { statusVariant } from "../lib/assessments";

interface SubmissionsDialogProps {
  assignment: Assignment | null;
  open: boolean;
  onClose: () => void;
}

/** Reviews and marks every submission for one assignment. */
export function SubmissionsDialog({ assignment, open, onClose }: SubmissionsDialogProps) {
  const { data: submissions, isLoading } = useSubmissions(open ? assignment?.id : undefined);
  const grade = useGradeSubmission();

  const [drafts, setDrafts] = useState<Record<string, { points: string; feedback: string }>>({});

  // Seed each row's inputs from whatever mark it already carries.
  useEffect(() => {
    if (!submissions) return;
    setDrafts(
      Object.fromEntries(
        submissions.map((s) => [
          s.id,
          { points: s.pointsAwarded?.toString() ?? "", feedback: s.feedback ?? "" },
        ]),
      ),
    );
  }, [submissions]);

  if (!assignment) return null;

  const save = (submissionId: string) => {
    const draft = drafts[submissionId];
    if (!draft) return;
    grade.mutate({
      submissionId,
      pointsAwarded: Number(draft.points),
      feedback: draft.feedback.trim() || null,
    });
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Submissions"
      description={`${assignment.title} · out of ${assignment.maxPoints} points`}
      className="max-w-3xl"
    >
      <div className="space-y-4">
        {grade.isError ? <Alert variant="error">{getApiErrorMessage(grade.error)}</Alert> : null}

        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-28 rounded-xl" />
            ))}
          </div>
        ) : submissions && submissions.length > 0 ? (
          <ul className="space-y-3">
            {submissions.map((submission) => {
              const draft = drafts[submission.id] ?? { points: "", feedback: "" };
              const points = Number(draft.points);
              const invalid =
                draft.points === "" || Number.isNaN(points) || points < 0 || points > assignment.maxPoints;

              return (
                <li key={submission.id} className="rounded-xl border border-border p-4">
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <div className="min-w-0">
                      <p className="truncate font-medium">{submission.studentName}</p>
                      <p className="truncate text-xs text-muted-foreground">{submission.studentEmail}</p>
                    </div>
                    <div className="flex flex-wrap items-center gap-1.5">
                      {submission.isLate && (
                        <Badge variant="destructive">
                          <Clock className="h-3 w-3" aria-hidden />
                          Late
                        </Badge>
                      )}
                      <Badge variant={statusVariant[submission.status]}>{submission.status}</Badge>
                      <span className="text-xs text-muted-foreground">
                        {timeAgo(submission.submittedAtUtc)}
                      </span>
                    </div>
                  </div>

                  <p className="mt-3 whitespace-pre-wrap break-words rounded-lg bg-muted/50 p-3 text-sm">
                    {submission.content}
                  </p>

                  {submission.attachmentUrl && (
                    <a
                      href={submission.attachmentUrl}
                      target="_blank"
                      rel="noreferrer noopener"
                      className="mt-2 inline-flex items-center gap-1.5 text-xs font-medium text-primary hover:underline"
                    >
                      <ExternalLink className="h-3.5 w-3.5" aria-hidden />
                      Open attachment
                    </a>
                  )}

                  <div className="mt-3 flex flex-wrap items-end gap-2">
                    <div className="w-28 space-y-1">
                      <label
                        htmlFor={`points-${submission.id}`}
                        className="text-xs font-medium text-muted-foreground"
                      >
                        Points
                      </label>
                      <Input
                        id={`points-${submission.id}`}
                        type="number"
                        min={0}
                        max={assignment.maxPoints}
                        value={draft.points}
                        onChange={(e) =>
                          setDrafts({ ...drafts, [submission.id]: { ...draft, points: e.target.value } })
                        }
                      />
                    </div>
                    <div className="min-w-[200px] flex-1 space-y-1">
                      <label
                        htmlFor={`feedback-${submission.id}`}
                        className="text-xs font-medium text-muted-foreground"
                      >
                        Feedback
                      </label>
                      <Textarea
                        id={`feedback-${submission.id}`}
                        rows={2}
                        value={draft.feedback}
                        onChange={(e) =>
                          setDrafts({ ...drafts, [submission.id]: { ...draft, feedback: e.target.value } })
                        }
                        placeholder="Optional"
                        maxLength={4000}
                      />
                    </div>
                    <Button
                      size="sm"
                      onClick={() => save(submission.id)}
                      disabled={invalid || grade.isPending}
                    >
                      {submission.status === "Graded" ? "Update mark" : "Save mark"}
                    </Button>
                  </div>
                </li>
              );
            })}
          </ul>
        ) : (
          <div className="flex flex-col items-center gap-2 py-10 text-center">
            <span className="rounded-2xl bg-muted p-3 text-muted-foreground">
              <Inbox className="h-6 w-6" aria-hidden />
            </span>
            <p className="font-medium">Nothing handed in yet.</p>
            <p className="text-sm text-muted-foreground">
              Submissions appear here as learners send them.
            </p>
          </div>
        )}

        <div className="flex justify-end pt-2">
          <Button variant="outline" onClick={onClose}>
            Close
          </Button>
        </div>
      </div>
    </Modal>
  );
}
