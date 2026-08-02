import { useEffect, useState } from "react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import type { Assignment } from "../api/types";
import { formatDue } from "../lib/assessments";

interface SubmitWorkDialogProps {
  assignment: Assignment | null;
  open: boolean;
  onClose: () => void;
  onSubmit: (content: string, attachmentUrl: string | null) => void;
  isSaving: boolean;
  error: unknown;
}

/** Where a learner writes their answer. Re-submitting replaces the previous attempt. */
export function SubmitWorkDialog({
  assignment,
  open,
  onClose,
  onSubmit,
  isSaving,
  error,
}: SubmitWorkDialogProps) {
  const [content, setContent] = useState("");
  const [attachmentUrl, setAttachmentUrl] = useState("");

  useEffect(() => {
    setContent(assignment?.mySubmission?.content ?? "");
    setAttachmentUrl(assignment?.mySubmission?.attachmentUrl ?? "");
  }, [assignment, open]);

  if (!assignment) return null;

  const isResubmit = assignment.mySubmission !== null;
  const wasGraded = assignment.mySubmission?.status === "Graded";

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={isResubmit ? "Replace your submission" : "Submit your work"}
      description={`${assignment.title} · due ${formatDue(assignment.dueAtUtc)}`}
    >
      <div className="space-y-4">
        {error ? <Alert variant="error">{getApiErrorMessage(error)}</Alert> : null}

        {wasGraded && (
          <Alert variant="error">
            This work has already been marked. Submitting again clears the mark and sends it back
            for review.
          </Alert>
        )}

        <div className="space-y-1.5">
          <Label htmlFor="submission-content">Your answer</Label>
          <Textarea
            id="submission-content"
            rows={8}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Write your answer here."
            maxLength={20000}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="submission-url">Attachment link</Label>
          <Input
            id="submission-url"
            value={attachmentUrl}
            onChange={(e) => setAttachmentUrl(e.target.value)}
            placeholder="https://drive.example.com/my-work"
            maxLength={1024}
          />
          <p className="text-xs text-muted-foreground">
            Optional. Link to work hosted elsewhere; direct file upload is not available yet.
          </p>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button
            onClick={() => onSubmit(content.trim(), attachmentUrl.trim() || null)}
            isLoading={isSaving}
            disabled={content.trim().length === 0}
          >
            {isResubmit ? "Replace submission" : "Submit"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
