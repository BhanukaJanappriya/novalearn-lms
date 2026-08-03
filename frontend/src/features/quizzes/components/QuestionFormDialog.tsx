import { useEffect, useState } from "react";
import { Plus, Trash2 } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import type { AuthoringQuestion, QuestionType, SaveQuestionInput } from "../api/types";
import { defaultOptionsFor, questionTypeLabels } from "../lib/quizzes";

interface QuestionFormDialogProps {
  question: AuthoringQuestion | null;
  open: boolean;
  onClose: () => void;
  onSubmit: (input: SaveQuestionInput) => void;
  isSaving: boolean;
  error: unknown;
}

interface OptionDraft {
  text: string;
  isCorrect: boolean;
}

/**
 * Creates or replaces a question. Options are edited as a set and saved wholesale, matching the
 * server: a question with no correct answer, or two, cannot be marked.
 */
export function QuestionFormDialog({
  question,
  open,
  onClose,
  onSubmit,
  isSaving,
  error,
}: QuestionFormDialogProps) {
  const [text, setText] = useState("");
  const [type, setType] = useState<QuestionType>("MultipleChoice");
  const [points, setPoints] = useState(10);
  const [options, setOptions] = useState<OptionDraft[]>(defaultOptionsFor("MultipleChoice"));
  const [acceptedAnswers, setAcceptedAnswers] = useState("");

  useEffect(() => {
    if (question) {
      setText(question.text);
      setType(question.type);
      setPoints(question.points);
      setOptions(
        question.options.length > 0
          ? question.options.map((o) => ({ text: o.text, isCorrect: o.isCorrect }))
          : defaultOptionsFor(question.type),
      );
      setAcceptedAnswers(question.acceptedAnswers.join("\n"));
    } else {
      setText("");
      setType("MultipleChoice");
      setPoints(10);
      setOptions(defaultOptionsFor("MultipleChoice"));
      setAcceptedAnswers("");
    }
  }, [question, open]);

  const changeType = (next: QuestionType) => {
    setType(next);
    // True or false has fixed options; the others start from a blank pair.
    if (next !== "ShortAnswer") {
      setOptions(defaultOptionsFor(next));
    }
  };

  /** Exactly one option is correct, so choosing one clears the rest. */
  const markCorrect = (index: number) =>
    setOptions(options.map((o, i) => ({ ...o, isCorrect: i === index })));

  const setOptionText = (index: number, value: string) =>
    setOptions(options.map((o, i) => (i === index ? { ...o, text: value } : o)));

  const addOption = () => setOptions([...options, { text: "", isCorrect: false }]);

  const removeOption = (index: number) => {
    const next = options.filter((_, i) => i !== index);
    // Never leave the set without a correct answer.
    if (!next.some((o) => o.isCorrect) && next.length > 0) {
      next[0] = { ...next[0], isCorrect: true };
    }
    setOptions(next);
  };

  const answerList = acceptedAnswers
    .split("\n")
    .map((a) => a.trim())
    .filter(Boolean);

  const isShortAnswer = type === "ShortAnswer";
  const filledOptions = options.filter((o) => o.text.trim().length > 0);

  const invalid =
    text.trim().length === 0 ||
    (isShortAnswer
      ? answerList.length === 0
      : filledOptions.length < 2 || filledOptions.filter((o) => o.isCorrect).length !== 1);

  const submit = () =>
    onSubmit({
      questionId: question?.id,
      text: text.trim(),
      type,
      points,
      acceptedAnswers: isShortAnswer ? answerList : [],
      options: isShortAnswer ? [] : filledOptions,
    });

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={question ? "Edit question" : "New question"}
      description={questionTypeLabels[type]}
    >
      <div className="space-y-4">
        {error ? <Alert variant="error">{getApiErrorMessage(error)}</Alert> : null}

        <div className="space-y-1.5">
          <Label htmlFor="question-text">Question</Label>
          <Textarea
            id="question-text"
            rows={3}
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="What does CPU stand for?"
            maxLength={2000}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="question-type">Type</Label>
            <select
              id="question-type"
              value={type}
              onChange={(e) => changeType(e.target.value as QuestionType)}
              className="h-10 w-full rounded-md border border-input bg-card px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              {Object.entries(questionTypeLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="question-points">Points</Label>
            <Input
              id="question-points"
              type="number"
              min={1}
              max={100}
              value={points}
              onChange={(e) => setPoints(Number(e.target.value))}
            />
          </div>
        </div>

        {isShortAnswer ? (
          <div className="space-y-1.5">
            <Label htmlFor="question-accepted">Accepted answers</Label>
            <Textarea
              id="question-accepted"
              rows={4}
              value={acceptedAnswers}
              onChange={(e) => setAcceptedAnswers(e.target.value)}
              placeholder={"OpenMP\nOpen MP"}
              maxLength={2000}
            />
            <p className="text-xs text-muted-foreground">
              One per line. Matching ignores capitals and surrounding spaces.
            </p>
          </div>
        ) : (
          <div className="space-y-2">
            <Label>Options</Label>
            <ul className="space-y-2">
              {options.map((option, index) => (
                <li key={index} className="flex items-center gap-2">
                  <input
                    type="radio"
                    name="correct-option"
                    checked={option.isCorrect}
                    onChange={() => markCorrect(index)}
                    aria-label={`Mark option ${index + 1} correct`}
                    className="h-4 w-4 shrink-0 accent-primary"
                  />
                  <Input
                    value={option.text}
                    onChange={(e) => setOptionText(index, e.target.value)}
                    placeholder={`Option ${index + 1}`}
                    maxLength={1000}
                    disabled={type === "TrueFalse"}
                  />
                  {type !== "TrueFalse" && options.length > 2 && (
                    <button
                      type="button"
                      onClick={() => removeOption(index)}
                      aria-label={`Remove option ${index + 1}`}
                      className="shrink-0 rounded-lg p-2 text-muted-foreground transition-colors hover:bg-muted hover:text-destructive"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  )}
                </li>
              ))}
            </ul>
            {type !== "TrueFalse" && (
              <Button variant="outline" size="sm" onClick={addOption}>
                <Plus className="h-3.5 w-3.5" />
                Add option
              </Button>
            )}
            <p className="text-xs text-muted-foreground">
              Select the radio button next to the correct answer.
            </p>
          </div>
        )}

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button onClick={submit} isLoading={isSaving} disabled={invalid}>
            {question ? "Save question" : "Add question"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
