import { useEffect, useState } from "react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import type { Assignment, AssignmentInput, AssessmentStatus } from "../api/types";
import { fromLocalInputValue, toLocalInputValue } from "../lib/assessments";

interface AssignmentFormDialogProps {
  /** Null creates a new assignment; an assignment edits it. */
  assignment: Assignment | null;
  open: boolean;
  onClose: () => void;
  onSubmit: (input: AssignmentInput) => void;
  isSaving: boolean;
  error: unknown;
}

const emptyForm = {
  title: "",
  instructions: "",
  dueAt: "",
  maxPoints: 100,
  allowLateSubmissions: false,
  status: "Draft" as AssessmentStatus,
};

/** Handles both creating and editing, mirroring CourseFormDialog. */
export function AssignmentFormDialog({
  assignment,
  open,
  onClose,
  onSubmit,
  isSaving,
  error,
}: AssignmentFormDialogProps) {
  const [form, setForm] = useState(emptyForm);

  // Re-seed whenever a different assignment is opened, including back to blank for create.
  useEffect(() => {
    setForm(
      assignment
        ? {
            title: assignment.title,
            instructions: assignment.instructions ?? "",
            dueAt: toLocalInputValue(assignment.dueAtUtc),
            maxPoints: assignment.maxPoints,
            allowLateSubmissions: assignment.allowLateSubmissions,
            status: assignment.status,
          }
        : emptyForm,
    );
  }, [assignment, open]);

  const submit = () =>
    onSubmit({
      title: form.title.trim(),
      instructions: form.instructions.trim() || null,
      dueAtUtc: fromLocalInputValue(form.dueAt),
      maxPoints: form.maxPoints,
      allowLateSubmissions: form.allowLateSubmissions,
      status: form.status,
    });

  const invalid = form.title.trim().length === 0 || form.maxPoints < 1;

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={assignment ? "Edit assignment" : "New assignment"}
      description={assignment ? assignment.title : "Learners see it once it is published."}
    >
      <div className="space-y-4">
        {error ? <Alert variant="error">{getApiErrorMessage(error)}</Alert> : null}

        <div className="space-y-1.5">
          <Label htmlFor="assignment-title">Title</Label>
          <Input
            id="assignment-title"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            placeholder="Problem set 1"
            maxLength={200}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="assignment-instructions">Instructions</Label>
          <Textarea
            id="assignment-instructions"
            value={form.instructions}
            onChange={(e) => setForm({ ...form, instructions: e.target.value })}
            placeholder="What learners need to do, and how it will be marked."
            rows={5}
            maxLength={20000}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="assignment-due">Due date</Label>
            <Input
              id="assignment-due"
              type="datetime-local"
              value={form.dueAt}
              onChange={(e) => setForm({ ...form, dueAt: e.target.value })}
            />
            <p className="text-xs text-muted-foreground">Leave blank for open ended work.</p>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="assignment-points">Points</Label>
            <Input
              id="assignment-points"
              type="number"
              min={1}
              max={1000}
              value={form.maxPoints}
              onChange={(e) => setForm({ ...form, maxPoints: Number(e.target.value) })}
            />
          </div>
        </div>

        <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-border p-3">
          <input
            type="checkbox"
            checked={form.allowLateSubmissions}
            onChange={(e) => setForm({ ...form, allowLateSubmissions: e.target.checked })}
            className="h-4 w-4 rounded border-border accent-primary"
          />
          <span className="text-sm">
            Accept late submissions
            <span className="block text-xs text-muted-foreground">
              Work handed in after the due date is accepted and flagged as late.
            </span>
          </span>
        </label>

        <div className="space-y-1.5">
          <Label htmlFor="assignment-status">Status</Label>
          <select
            id="assignment-status"
            value={form.status}
            onChange={(e) => setForm({ ...form, status: e.target.value as AssessmentStatus })}
            className="h-10 w-full rounded-md border border-input bg-card px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <option value="Draft">Draft, hidden from learners</option>
            <option value="Published">Published, open for submission</option>
          </select>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button onClick={submit} isLoading={isSaving} disabled={invalid}>
            {assignment ? "Save changes" : "Create assignment"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
