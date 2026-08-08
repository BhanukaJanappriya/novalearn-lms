import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  ArrowLeft,
  ArrowDown,
  ArrowUp,
  CircleAlert,
  CircleCheck,
  Copy,
  ListChecks,
  Pencil,
  PenLine,
  Plus,
  Trash2,
  TriangleAlert,
} from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import {
  useDeleteQuestion,
  useDuplicateQuestion,
  useQuizAuthoring,
  useReorderQuestions,
  useSaveQuestion,
} from "../api/queries";
import type { AuthoringQuestion, SaveQuestionInput } from "../api/types";
import { QuestionFormDialog } from "../components/QuestionFormDialog";
import { questionTypeLabels } from "../lib/quizzes";

/** Lecturer view: the question bank for one quiz, answer key included. */
export function QuizBuilderPage() {
  const { courseId = "", quizId = "" } = useParams();
  const { data, isLoading, isError, error } = useQuizAuthoring(quizId);

  const saveQuestion = useSaveQuestion(quizId);
  const deleteQuestion = useDeleteQuestion();
  const duplicateQuestion = useDuplicateQuestion();
  const reorderQuestions = useReorderQuestions(quizId);

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<AuthoringQuestion | null>(null);
  const [pendingDelete, setPendingDelete] = useState<AuthoringQuestion | null>(null);

  const save = (input: SaveQuestionInput) =>
    saveQuestion.mutate(input, { onSuccess: () => setFormOpen(false) });

  const confirmDelete = () => {
    if (!pendingDelete) return;
    deleteQuestion.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) });
  };

  /** Swaps a question with its neighbour and sends the whole new order. */
  const move = (index: number, direction: -1 | 1) => {
    if (!data) return;
    const ids = data.questions.map((q) => q.id);
    const target = index + direction;
    if (target < 0 || target >= ids.length) return;

    [ids[index], ids[target]] = [ids[target], ids[index]];
    reorderQuestions.mutate(ids);
  };

  return (
    <PageTransition>
      <div className="space-y-6">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <Link
            to={`/admin/courses/${courseId}/quizzes`}
            className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" aria-hidden />
            Back to quizzes
          </Link>
          <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <ListChecks className="h-6 w-6 text-primary" aria-hidden />
            {data?.quiz.title ?? "Questions"}
          </h1>
          {data && (
            <p className="mt-1 text-muted-foreground">
              {data.questions.length} question{data.questions.length === 1 ? "" : "s"} ·{" "}
              {data.quiz.totalPoints} points
            </p>
          )}
        </div>
        <Button
          size="sm"
          onClick={() => {
            setEditing(null);
            setFormOpen(true);
          }}
        >
          <Plus className="h-4 w-4" />
          Add question
        </Button>
      </header>

      {isError && (
        <Alert variant="error">{getApiErrorMessage(error, "We could not load this quiz.")}</Alert>
      )}
      {deleteQuestion.isError && (
        <Alert variant="error">{getApiErrorMessage(deleteQuestion.error)}</Alert>
      )}
      {reorderQuestions.isError && (
        <Alert variant="error">{getApiErrorMessage(reorderQuestions.error)}</Alert>
      )}
      {duplicateQuestion.isError && (
        <Alert variant="error">{getApiErrorMessage(duplicateQuestion.error)}</Alert>
      )}

      {data && !data.quiz.isReadyToPublish && (
        <div className="flex items-start gap-3 rounded-xl border border-[hsl(var(--warning))]/30 bg-warning/5 p-3 text-sm">
          <CircleAlert className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
          <p className="text-muted-foreground">
            This quiz cannot be published yet. Every question needs a correct answer, and there
            must be at least one question.
          </p>
        </div>
      )}

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-24 rounded-[18px]" />
          ))}
        </div>
      ) : data && data.questions.length === 0 ? (
        <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
          <p className="font-medium">No questions yet.</p>
          <p className="mt-1 text-sm text-muted-foreground">
            Add multiple choice, true or false, or short answer questions.
          </p>
        </div>
      ) : (
        <ol className="space-y-3">
          {data?.questions.map((question, index) => (
            <li
              key={question.id}
              className="rounded-[18px] border border-border bg-card p-4 shadow-soft"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="text-xs font-semibold text-muted-foreground">Q{index + 1}</span>
                    <Badge variant="outline">{questionTypeLabels[question.type]}</Badge>
                    <Badge variant="neutral">{question.points} pts</Badge>
                    {question.isRequired && <Badge variant="default">Required</Badge>}
                    {question.requiresManualMarking && (
                      <Badge variant="warning">
                        <PenLine className="h-3 w-3" aria-hidden />
                        Marked by hand
                      </Badge>
                    )}
                    {!question.isAnswerable && (
                      <Badge variant="warning">
                        <CircleAlert className="h-3 w-3" aria-hidden />
                        Incomplete
                      </Badge>
                    )}
                  </div>
                  <p className="mt-2 whitespace-pre-wrap font-medium">{question.text}</p>

                  {question.type === "Essay" ? (
                    <p className="mt-2 text-xs text-muted-foreground">
                      {question.markingGuidance
                        ? `Guidance: ${question.markingGuidance}`
                        : "No marking guidance set."}
                    </p>
                  ) : question.type === "ShortAnswer" ? (
                    <p className="mt-2 text-xs text-muted-foreground">
                      Accepts:{" "}
                      <span className="text-success">{question.acceptedAnswers.join(", ") || "nothing yet"}</span>
                    </p>
                  ) : (
                    <ul className="mt-2 space-y-1">
                      {question.options.map((option) => (
                        <li
                          key={option.id}
                          className={`flex items-center gap-2 text-sm ${
                            option.isCorrect ? "text-success" : "text-muted-foreground"
                          }`}
                        >
                          {option.isCorrect ? (
                            <CircleCheck className="h-3.5 w-3.5 shrink-0" aria-hidden />
                          ) : (
                            <span className="h-3.5 w-3.5 shrink-0 rounded-full border border-current opacity-40" />
                          )}
                          {option.text}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>

                <div className="flex flex-wrap items-center gap-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => move(index, -1)}
                    disabled={index === 0 || reorderQuestions.isPending}
                    aria-label={`Move question ${index + 1} up`}
                  >
                    <ArrowUp className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => move(index, 1)}
                    disabled={index === (data?.questions.length ?? 0) - 1 || reorderQuestions.isPending}
                    aria-label={`Move question ${index + 1} down`}
                  >
                    <ArrowDown className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => duplicateQuestion.mutate(question.id)}
                    disabled={duplicateQuestion.isPending}
                  >
                    <Copy className="h-3.5 w-3.5" />
                    Duplicate
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => {
                      setEditing(question);
                      setFormOpen(true);
                    }}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                    Edit
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => setPendingDelete(question)}>
                    <Trash2 className="h-3.5 w-3.5" />
                    Delete
                  </Button>
                </div>
              </div>
            </li>
          ))}
        </ol>
      )}

      <QuestionFormDialog
        question={editing}
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={save}
        isSaving={saveQuestion.isPending}
        error={saveQuestion.error}
      />

      <Modal
        open={pendingDelete !== null}
        onClose={() => setPendingDelete(null)}
        title="Delete question"
        description={pendingDelete?.text}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-destructive" aria-hidden />
            <p className="text-muted-foreground">
              Attempts already marked keep their recorded score.
            </p>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setPendingDelete(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={confirmDelete}
              isLoading={deleteQuestion.isPending}
            >
              Delete
            </Button>
          </div>
        </div>
      </Modal>
    </div>
    </PageTransition>
  );
}
