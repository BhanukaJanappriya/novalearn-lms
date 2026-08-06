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
import {
  defaultOptionsFor,
  isOptionBased,
  questionTypeHints,
  questionTypeLabels,
  requiresManualMarking,
} from "../lib/quizzes";

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
 * server: a question with no correct answer cannot be marked.
 *
 * The form mirrors the server's rules per type, so it will not let the author submit something
 * the API would reject.
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
  const [isRequired, setIsRequired] = useState(false);
  const [options, setOptions] = useState<OptionDraft[]>(defaultOptionsFor("MultipleChoice"));
  const [acceptedAnswers, setAcceptedAnswers] = useState("");
  const [markingGuidance, setMarkingGuidance] = useState("");

  useEffect(() => {
    if (question) {
      setText(question.text);
      setType(question.type);
      setPoints(question.points);
      setIsRequired(question.isRequired);
      setOptions(
        question.options.length > 0
          ? question.options.map((o) => ({ text: o.text, isCorrect: o.isCorrect }))
          : defaultOptionsFor(question.type),
      );
      setAcceptedAnswers(question.acceptedAnswers.join("\n"));
      setMarkingGuidance(question.markingGuidance ?? "");
    } else {
      setText("");
      setType("MultipleChoice");
      setPoints(10);
      setIsRequired(false);
      setOptions(defaultOptionsFor("MultipleChoice"));
      setAcceptedAnswers("");
      setMarkingGuidance("");
    }
  }, [question, open]);

  const changeType = (next: QuestionType) => {
    setType(next);
    // True or false has fixed options; the others start from a fresh default for the new type.
    setOptions(defaultOptionsFor(next));
  };

  /**
   * Single-answer types allow exactly one key, so choosing one clears the rest. Checkboxes
   * toggle independently.
   */
  const toggleCorrect = (index: number) =>
    setOptions(
      options.map((o, i) =>
        type === "MultipleResponse"
          ? i === index
            ? { ...o, isCorrect: !o.isCorrect }
            : o
          : { ...o, isCorrect: i === index },
      ),
    );

  const setOptionText = (index: number, value: string) =>
    setOptions(options.map((o, i) => (i === index ? { ...o, text: value } : o)));

  const addOption = () => setOptions([...options, { text: "", isCorrect: false }]);

  const removeOption = (index: number) => {
    const next = options.filter((_, i) => i !== index);
    // A single-answer question must never be left without a key.
    if (type !== "MultipleResponse" && !next.some((o) => o.isCorrect) && next.length > 0) {
      next[0] = { ...next[0], isCorrect: true };
    }
    setOptions(next);
  };

  const answerList = acceptedAnswers
    .split("\n")
    .map((a) => a.trim())
    .filter(Boolean);

  const optionBased = isOptionBased(type);
  const isEssay = requiresManualMarking(type);
  const isShortAnswer = type === "ShortAnswer";
  const filledOptions = options.filter((o) => o.text.trim().length > 0);
  const correctCount = filledOptions.filter((o) => o.isCorrect).length;

  const invalid =
    text.trim().length === 0 ||
    (isShortAnswer && answerList.length === 0) ||
    (optionBased &&
      (filledOptions.length < 2 ||
        (type === "MultipleResponse" ? correctCount < 1 : correctCount !== 1)));

  const submit = () =>
    onSubmit({
      questionId: question?.id,
      text: text.trim(),
      type,
      points,
      acceptedAnswers: isShortAnswer ? answerList : [],
      options: optionBased ? filledOptions : [],
      isRequired,
      markingGuidance: isEssay ? markingGuidance.trim() || null : null,
    });

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={question ? "Edit question" : "New question"}
      description={questionTypeHints[type]}
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

        <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-border p-3">
          <input
            type="checkbox"
            checked={isRequired}
            onChange={(e) => setIsRequired(e.target.checked)}
            className="h-4 w-4 rounded border-border accent-primary"
          />
          <span className="text-sm">
            Required
            <span className="block text-xs text-muted-foreground">
              Learners cannot submit the quiz until they answer this.
            </span>
          </span>
        </label>

        {isShortAnswer && (
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
              One per line. Matching ignores capitals and surrounding spaces, and any one of them
              counts as correct.
            </p>
          </div>
        )}

        {isEssay && (
          <div className="space-y-1.5">
            <Label htmlFor="question-guidance">Marking guidance</Label>
            <Textarea
              id="question-guidance"
              rows={3}
              value={markingGuidance}
              onChange={(e) => setMarkingGuidance(e.target.value)}
              placeholder="What a full-mark answer covers."
              maxLength={2000}
            />
            <p className="text-xs text-muted-foreground">
              Optional, and only ever shown to whoever marks this. Learners never see it.
            </p>
          </div>
        )}

        {optionBased && (
          <div className="space-y-2">
            <Label>Options</Label>
            <ul className="space-y-2">
              {options.map((option, index) => (
                <li key={index} className="flex items-center gap-2">
                  <input
                    type={type === "MultipleResponse" ? "checkbox" : "radio"}
                    name={type === "MultipleResponse" ? undefined : "correct-option"}
                    checked={option.isCorrect}
                    onChange={() => toggleCorrect(index)}
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
              {type === "MultipleResponse"
                ? "Tick every correct option. Learners must select exactly that set to score."
                : "Select the button next to the correct answer."}
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
