import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ArrowLeft, CheckCircle2, ClipboardList, Clock, ExternalLink } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useAssignments, useSubmitAssignment } from "../api/queries";
import type { Assignment } from "../api/types";
import { SubmitWorkDialog } from "../components/SubmitWorkDialog";
import { formatDue, isOverdue, learnerStatus } from "../lib/assessments";

/** Learner view: the published work on one course, and their own standing on each piece. */
export function MyAssignmentsPage() {
  const { courseId = "" } = useParams();
  const { data: assignments, isLoading, isError, error } = useAssignments(courseId);
  const submit = useSubmitAssignment();
  const [active, setActive] = useState<Assignment | null>(null);

  const send = (content: string, attachmentUrl: string | null) => {
    if (!active) return;
    submit.mutate(
      { assignmentId: active.id, content, attachmentUrl },
      { onSuccess: () => setActive(null) },
    );
  };

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-4xl px-6 py-10">
        <Link
          to="/my-courses"
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to my courses
        </Link>

        <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
          <ClipboardList className="h-6 w-6 text-primary" aria-hidden />
          Assignments
        </h1>
        <p className="mt-1 text-muted-foreground">Everything set for this course.</p>

        {isError && (
          <Alert variant="error" className="mt-6">
            {getApiErrorMessage(error, "We could not load your assignments.")}
          </Alert>
        )}

        {isLoading && (
          <div className="mt-8 space-y-2">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-28 rounded-[18px]" />
            ))}
          </div>
        )}

        {assignments && assignments.length === 0 && (
          <div className="mt-8 rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">No assignments yet.</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Your lecturer has not published any work for this course.
            </p>
          </div>
        )}

        <ul className="mt-8 space-y-3">
          {assignments?.map((assignment) => {
            const status = learnerStatus(assignment);
            const submission = assignment.mySubmission;

            return (
              <li
                key={assignment.id}
                className="rounded-[18px] border border-border bg-card p-4 shadow-soft"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <h2 className="truncate font-semibold">{assignment.title}</h2>
                      <Badge variant={status.variant}>{status.label}</Badge>
                      {submission?.isLate && <Badge variant="destructive">Late</Badge>}
                    </div>
                    <p className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-muted-foreground">
                      <span
                        className={`inline-flex items-center gap-1 ${
                          isOverdue(assignment) && !submission ? "text-[hsl(var(--warning))]" : ""
                        }`}
                      >
                        <Clock className="h-3.5 w-3.5" aria-hidden />
                        {formatDue(assignment.dueAtUtc)}
                      </span>
                      <span>{assignment.maxPoints} points</span>
                    </p>
                  </div>

                  <Button
                    size="sm"
                    variant={submission ? "outline" : "default"}
                    onClick={() => setActive(assignment)}
                    disabled={!assignment.isOpen}
                    title={!assignment.isOpen ? "This assignment is closed" : undefined}
                  >
                    {submission ? "Replace" : "Submit"}
                  </Button>
                </div>

                {assignment.instructions && (
                  <p className="mt-3 whitespace-pre-wrap text-sm text-muted-foreground">
                    {assignment.instructions}
                  </p>
                )}

                {submission?.status === "Graded" && (
                  <div className="mt-3 rounded-xl border border-[hsl(var(--success))]/30 bg-success/5 p-3">
                    <p className="flex items-center gap-2 text-sm font-medium text-success">
                      <CheckCircle2 className="h-4 w-4" aria-hidden />
                      {submission.pointsAwarded} out of {assignment.maxPoints}
                    </p>
                    {submission.feedback && (
                      <p className="mt-1.5 whitespace-pre-wrap text-sm text-muted-foreground">
                        {submission.feedback}
                      </p>
                    )}
                  </div>
                )}

                {submission?.attachmentUrl && (
                  <a
                    href={submission.attachmentUrl}
                    target="_blank"
                    rel="noreferrer noopener"
                    className="mt-2 inline-flex items-center gap-1.5 text-xs font-medium text-primary hover:underline"
                  >
                    <ExternalLink className="h-3.5 w-3.5" aria-hidden />
                    Your attachment
                  </a>
                )}
              </li>
            );
          })}
        </ul>
      </main>

      <SubmitWorkDialog
        assignment={active}
        open={active !== null}
        onClose={() => setActive(null)}
        onSubmit={send}
        isSaving={submit.isPending}
        error={submit.error}
      />
    </div>
  );
}
