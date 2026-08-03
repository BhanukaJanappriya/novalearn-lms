import { useEffect, useState } from "react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import type { AssessmentStatus, QuizInput, QuizSummary } from "../api/types";

interface QuizFormDialogProps {
  /** Null creates a quiz; a quiz edits it. */
  quiz: QuizSummary | null;
  open: boolean;
  onClose: () => void;
  onSubmit: (input: QuizInput) => void;
  isSaving: boolean;
  error: unknown;
}

const blank = {
  title: "",
  description: "",
  timed: false,
  timeLimitMinutes: 30,
  limitAttempts: true,
  maxAttempts: 2,
  hasPassMark: true,
  passingScorePercent: 60,
  shuffleQuestions: false,
  status: "Draft" as AssessmentStatus,
};

/** Handles both creating and editing, mirroring the assignment form. */
export function QuizFormDialog({ quiz, open, onClose, onSubmit, isSaving, error }: QuizFormDialogProps) {
  const [form, setForm] = useState(blank);

  useEffect(() => {
    setForm(
      quiz
        ? {
            title: quiz.title,
            description: quiz.description ?? "",
            timed: quiz.timeLimitMinutes !== null,
            timeLimitMinutes: quiz.timeLimitMinutes ?? 30,
            limitAttempts: quiz.maxAttempts !== null,
            maxAttempts: quiz.maxAttempts ?? 2,
            hasPassMark: quiz.passingScorePercent !== null,
            passingScorePercent: quiz.passingScorePercent ?? 60,
            shuffleQuestions: quiz.shuffleQuestions,
            status: quiz.status,
          }
        : blank,
    );
  }, [quiz, open]);

  const submit = () =>
    onSubmit({
      title: form.title.trim(),
      description: form.description.trim() || null,
      timeLimitMinutes: form.timed ? form.timeLimitMinutes : null,
      maxAttempts: form.limitAttempts ? form.maxAttempts : null,
      passingScorePercent: form.hasPassMark ? form.passingScorePercent : null,
      shuffleQuestions: form.shuffleQuestions,
      status: form.status,
    });

  // Publishing is only offered once the server says the quiz is ready.
  const canPublish = quiz?.isReadyToPublish ?? false;

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={quiz ? "Edit quiz" : "New quiz"}
      description={quiz ? quiz.title : "You will add questions next."}
    >
      <div className="space-y-4">
        {error ? <Alert variant="error">{getApiErrorMessage(error)}</Alert> : null}

        <div className="space-y-1.5">
          <Label htmlFor="quiz-title">Title</Label>
          <Input
            id="quiz-title"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            placeholder="Week 1 knowledge check"
            maxLength={200}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="quiz-description">Description</Label>
          <Textarea
            id="quiz-description"
            rows={3}
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            placeholder="What this quiz covers."
            maxLength={4000}
          />
        </div>

        <ToggleRow
          checked={form.timed}
          onChange={(timed) => setForm({ ...form, timed })}
          label="Time limit"
          hint="Learners see a countdown once they start."
        >
          <Input
            type="number"
            min={1}
            max={600}
            aria-label="Minutes allowed"
            value={form.timeLimitMinutes}
            onChange={(e) => setForm({ ...form, timeLimitMinutes: Number(e.target.value) })}
            className="w-24"
          />
        </ToggleRow>

        <ToggleRow
          checked={form.limitAttempts}
          onChange={(limitAttempts) => setForm({ ...form, limitAttempts })}
          label="Limit attempts"
          hint="Off means learners can retake it freely."
        >
          <Input
            type="number"
            min={1}
            aria-label="Attempts allowed"
            value={form.maxAttempts}
            onChange={(e) => setForm({ ...form, maxAttempts: Number(e.target.value) })}
            className="w-24"
          />
        </ToggleRow>

        <ToggleRow
          checked={form.hasPassMark}
          onChange={(hasPassMark) => setForm({ ...form, hasPassMark })}
          label="Pass mark"
          hint="Off means the quiz is scored but not pass or fail."
        >
          <div className="flex items-center gap-1.5">
            <Input
              type="number"
              min={0}
              max={100}
              aria-label="Pass mark percentage"
              value={form.passingScorePercent}
              onChange={(e) => setForm({ ...form, passingScorePercent: Number(e.target.value) })}
              className="w-20"
            />
            <span className="text-sm text-muted-foreground">%</span>
          </div>
        </ToggleRow>

        <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-border p-3">
          <input
            type="checkbox"
            checked={form.shuffleQuestions}
            onChange={(e) => setForm({ ...form, shuffleQuestions: e.target.checked })}
            className="h-4 w-4 rounded border-border accent-primary"
          />
          <span className="text-sm">
            Shuffle questions
            <span className="block text-xs text-muted-foreground">
              Each learner gets a different order, fixed for the whole attempt.
            </span>
          </span>
        </label>

        <div className="space-y-1.5">
          <Label htmlFor="quiz-status">Status</Label>
          <select
            id="quiz-status"
            value={form.status}
            onChange={(e) => setForm({ ...form, status: e.target.value as AssessmentStatus })}
            className="h-10 w-full rounded-md border border-input bg-card px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <option value="Draft">Draft, hidden from learners</option>
            <option value="Published" disabled={!canPublish}>
              Published, open for attempts
            </option>
          </select>
          {!canPublish && (
            <p className="text-xs text-muted-foreground">
              Add at least one question, each with a correct answer, before publishing.
            </p>
          )}
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button onClick={submit} isLoading={isSaving} disabled={form.title.trim().length === 0}>
            {quiz ? "Save changes" : "Create quiz"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}

function ToggleRow({
  checked,
  onChange,
  label,
  hint,
  children,
}: {
  checked: boolean;
  onChange: (value: boolean) => void;
  label: string;
  hint: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex items-center gap-3 rounded-xl border border-border p-3">
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        aria-label={label}
        className="h-4 w-4 shrink-0 rounded border-border accent-primary"
      />
      <span className="min-w-0 flex-1 text-sm">
        {label}
        <span className="block text-xs text-muted-foreground">{hint}</span>
      </span>
      {checked ? children : null}
    </div>
  );
}
