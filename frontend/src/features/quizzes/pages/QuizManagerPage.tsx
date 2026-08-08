import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  ArrowLeft,
  BarChart3,
  CircleAlert,
  Clock,
  ListChecks,
  Pencil,
  Plus,
  Repeat,
  Trash2,
  TriangleAlert,
} from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { LinkButton } from "@/components/ui/link-button";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import {
  useCreateQuiz,
  useDeleteQuiz,
  useQuizzes,
  useUpdateQuiz,
} from "../api/queries";
import type { QuizInput, QuizSummary } from "../api/types";
import { QuizFormDialog } from "../components/QuizFormDialog";
import { formatAttempts, formatTimeLimit } from "../lib/quizzes";

/** Lecturer view: the quizzes on one course. Question editing lives on the builder page. */
export function QuizManagerPage() {
  const { courseId = "" } = useParams();
  const { data: quizzes, isLoading, isError, error } = useQuizzes(courseId);

  const createQuiz = useCreateQuiz(courseId);
  const updateQuiz = useUpdateQuiz();
  const deleteQuiz = useDeleteQuiz();

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<QuizSummary | null>(null);
  const [pendingDelete, setPendingDelete] = useState<QuizSummary | null>(null);

  const openCreate = () => {
    setEditing(null);
    setFormOpen(true);
  };

  const save = (input: QuizInput) => {
    const onSuccess = () => setFormOpen(false);
    if (editing) {
      updateQuiz.mutate({ quizId: editing.id, input }, { onSuccess });
    } else {
      createQuiz.mutate(input, { onSuccess });
    }
  };

  const confirmDelete = () => {
    if (!pendingDelete) return;
    deleteQuiz.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) });
  };

  return (
    <PageTransition>
      <div className="space-y-6">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <Link
            to="/admin/courses"
            className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" aria-hidden />
            Back to courses
          </Link>
          <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <ListChecks className="h-6 w-6 text-primary" aria-hidden />
            Quizzes
          </h1>
          <p className="mt-1 text-muted-foreground">
            Self marking tests. Learners see them once published.
          </p>
        </div>
        <Button size="sm" onClick={openCreate}>
          <Plus className="h-4 w-4" />
          New quiz
        </Button>
      </header>

      {isError && <Alert variant="error">{getApiErrorMessage(error, "We could not load quizzes.")}</Alert>}
      {deleteQuiz.isError && <Alert variant="error">{getApiErrorMessage(deleteQuiz.error)}</Alert>}
      {updateQuiz.isError && !formOpen && (
        <Alert variant="error">{getApiErrorMessage(updateQuiz.error)}</Alert>
      )}

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-[18px]" />
          ))}
        </div>
      ) : quizzes && quizzes.length === 0 ? (
        <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
          <p className="font-medium">No quizzes yet.</p>
          <p className="mt-1 text-sm text-muted-foreground">
            Create one, add questions, then publish it.
          </p>
          <Button className="mt-4" size="sm" onClick={openCreate}>
            <Plus className="h-4 w-4" />
            New quiz
          </Button>
        </div>
      ) : (
        <ul className="space-y-3">
          {quizzes?.map((quiz) => (
            <li key={quiz.id} className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-2">
                    <h2 className="truncate font-semibold">{quiz.title}</h2>
                    <Badge variant={quiz.status === "Published" ? "success" : "neutral"}>
                      {quiz.status}
                    </Badge>
                    {!quiz.isReadyToPublish && (
                      <Badge variant="warning">
                        <CircleAlert className="h-3 w-3" aria-hidden />
                        Needs questions
                      </Badge>
                    )}
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
                      {formatAttempts(quiz.maxAttempts)}
                    </span>
                    {quiz.passingScorePercent !== null && <span>Pass at {quiz.passingScorePercent}%</span>}
                  </p>
                </div>

                <div className="flex flex-wrap items-center gap-1">
                  <LinkButton
                    to={`/admin/courses/${courseId}/quizzes/${quiz.id}`}
                    variant="ghost"
                    size="sm"
                  >
                    <ListChecks className="h-3.5 w-3.5" />
                    Questions
                  </LinkButton>
                  <LinkButton
                    to={`/admin/courses/${courseId}/quizzes/${quiz.id}/results`}
                    variant="ghost"
                    size="sm"
                  >
                    <BarChart3 className="h-3.5 w-3.5" />
                    Results
                  </LinkButton>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => {
                      setEditing(quiz);
                      setFormOpen(true);
                    }}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                    Edit
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => setPendingDelete(quiz)}>
                    <Trash2 className="h-3.5 w-3.5" />
                    Delete
                  </Button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}

      <QuizFormDialog
        quiz={editing}
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={save}
        isSaving={createQuiz.isPending || updateQuiz.isPending}
        error={editing ? updateQuiz.error : createQuiz.error}
      />

      <Modal
        open={pendingDelete !== null}
        onClose={() => setPendingDelete(null)}
        title="Delete quiz"
        description={pendingDelete?.title}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-destructive" aria-hidden />
            <p className="text-muted-foreground">
              The quiz and its questions are withdrawn. Results already recorded are kept.
            </p>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setPendingDelete(null)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={confirmDelete} isLoading={deleteQuiz.isPending}>
              Delete
            </Button>
          </div>
        </div>
      </Modal>
    </div>
    </PageTransition>
  );
}
