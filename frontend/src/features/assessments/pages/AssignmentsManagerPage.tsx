import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ArrowLeft, ClipboardList, Clock, Inbox, Pencil, Plus, Table2, Trash2, TriangleAlert } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { LinkButton } from "@/components/ui/link-button";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import {
  useAssignments,
  useCreateAssignment,
  useDeleteAssignment,
  useUpdateAssignment,
} from "../api/queries";
import type { Assignment, AssignmentInput } from "../api/types";
import { AssignmentFormDialog } from "../components/AssignmentFormDialog";
import { SubmissionsDialog } from "../components/SubmissionsDialog";
import { formatDue, isOverdue } from "../lib/assessments";

/** Lecturer view: author assignments for one course and mark what comes back. */
export function AssignmentsManagerPage() {
  const { courseId = "" } = useParams();
  const { data: assignments, isLoading, isError, error } = useAssignments(courseId);

  const createAssignment = useCreateAssignment(courseId);
  const updateAssignment = useUpdateAssignment();
  const deleteAssignment = useDeleteAssignment();

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<Assignment | null>(null);
  const [reviewing, setReviewing] = useState<Assignment | null>(null);
  const [pendingDelete, setPendingDelete] = useState<Assignment | null>(null);

  const openCreate = () => {
    setEditing(null);
    setFormOpen(true);
  };

  const openEdit = (assignment: Assignment) => {
    setEditing(assignment);
    setFormOpen(true);
  };

  const save = (input: AssignmentInput) => {
    const onSuccess = () => setFormOpen(false);
    if (editing) {
      updateAssignment.mutate({ assignmentId: editing.id, input }, { onSuccess });
    } else {
      createAssignment.mutate(input, { onSuccess });
    }
  };

  const confirmDelete = () => {
    if (!pendingDelete) return;
    deleteAssignment.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) });
  };

  const saving = createAssignment.isPending || updateAssignment.isPending;
  const formError = editing ? updateAssignment.error : createAssignment.error;

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
            <ClipboardList className="h-6 w-6 text-primary" aria-hidden />
            Assignments
          </h1>
          <p className="mt-1 text-muted-foreground">
            Set work, then review and mark what learners hand in.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <LinkButton to={`/admin/courses/${courseId}/gradebook`} variant="outline" size="sm">
            <Table2 className="h-4 w-4" />
            Gradebook
          </LinkButton>
          <Button size="sm" onClick={openCreate}>
            <Plus className="h-4 w-4" />
            New assignment
          </Button>
        </div>
      </header>

      {isError && (
        <Alert variant="error">{getApiErrorMessage(error, "We could not load assignments.")}</Alert>
      )}
      {deleteAssignment.isError && (
        <Alert variant="error">{getApiErrorMessage(deleteAssignment.error)}</Alert>
      )}

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-24 rounded-[18px]" />
          ))}
        </div>
      ) : assignments && assignments.length === 0 ? (
        <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
          <p className="font-medium">No assignments yet.</p>
          <p className="mt-1 text-sm text-muted-foreground">
            Create your first one to start collecting work.
          </p>
          <Button className="mt-4" size="sm" onClick={openCreate}>
            <Plus className="h-4 w-4" />
            New assignment
          </Button>
        </div>
      ) : (
        <ul className="space-y-3">
          {assignments?.map((assignment) => (
            <li
              key={assignment.id}
              className="rounded-[18px] border border-border bg-card p-4 shadow-soft"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-2">
                    <h2 className="truncate font-semibold">{assignment.title}</h2>
                    <Badge variant={assignment.status === "Published" ? "success" : "neutral"}>
                      {assignment.status}
                    </Badge>
                    {assignment.allowLateSubmissions && <Badge variant="outline">Late allowed</Badge>}
                    {!assignment.isOpen && assignment.status === "Published" && (
                      <Badge variant="destructive">Closed</Badge>
                    )}
                  </div>
                  <p className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
                    <span
                      className={`inline-flex items-center gap-1 ${
                        isOverdue(assignment) ? "text-[hsl(var(--warning))]" : ""
                      }`}
                    >
                      <Clock className="h-3.5 w-3.5" aria-hidden />
                      {formatDue(assignment.dueAtUtc)}
                    </span>
                    <span>{assignment.maxPoints} points</span>
                    <span className="inline-flex items-center gap-1">
                      <Inbox className="h-3.5 w-3.5" aria-hidden />
                      {assignment.submissionCount} submitted · {assignment.gradedCount} marked
                    </span>
                  </p>
                </div>

                <div className="flex items-center gap-1">
                  <Button variant="ghost" size="sm" onClick={() => setReviewing(assignment)}>
                    <Inbox className="h-3.5 w-3.5" />
                    Review
                    {assignment.submissionCount > assignment.gradedCount && (
                      <span className="ml-1 rounded-full bg-primary/15 px-1.5 text-xs font-semibold text-primary">
                        {assignment.submissionCount - assignment.gradedCount}
                      </span>
                    )}
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => openEdit(assignment)}>
                    <Pencil className="h-3.5 w-3.5" />
                    Edit
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => setPendingDelete(assignment)}>
                    <Trash2 className="h-3.5 w-3.5" />
                    Delete
                  </Button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}

      <AssignmentFormDialog
        assignment={editing}
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={save}
        isSaving={saving}
        error={formError}
      />

      <SubmissionsDialog
        assignment={reviewing}
        open={reviewing !== null}
        onClose={() => setReviewing(null)}
      />

      <Modal
        open={pendingDelete !== null}
        onClose={() => setPendingDelete(null)}
        title="Delete assignment"
        description={pendingDelete?.title}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-destructive" aria-hidden />
            <p className="text-muted-foreground">
              Learners will no longer see this assignment. Submissions already handed in are kept
              but stop counting towards the gradebook.
            </p>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setPendingDelete(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={confirmDelete}
              isLoading={deleteAssignment.isPending}
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
