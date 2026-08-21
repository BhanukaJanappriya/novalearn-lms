import { Link, useParams } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { useAuth } from "@/context/AuthContext";
import { getApiErrorMessage } from "@/lib/apiError";
import {
  useAssignTicket,
  useChangeTicketPriority,
  useChangeTicketStatus,
  useReplyToTicket,
  useTicket,
} from "../api/queries";
import type { TicketPriority, TicketStatus } from "../api/types";
import { ReplyBox } from "../components/ReplyBox";
import { TicketThread } from "../components/TicketThread";
import { allPriorities, allStatuses, categoryLabel, statusLabel } from "../lib/support";

/** Staff's view of a ticket: the full thread including internal notes, and the triage controls. */
export function StaffTicketDetailPage() {
  const { ticketId } = useParams<{ ticketId: string }>();
  const { user } = useAuth();
  const { data: ticket, isLoading, isError, error } = useTicket(ticketId);
  const reply = useReplyToTicket(ticketId ?? "");
  const changeStatus = useChangeTicketStatus(ticketId ?? "");
  const changePriority = useChangeTicketPriority(ticketId ?? "");
  const assign = useAssignTicket(ticketId ?? "");

  const isMine = user && ticket?.assignedToId === user.id;

  return (
    <PageTransition>
      <div className="mx-auto max-w-3xl space-y-4">
        <Link
          to="/admin/support"
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to queue
        </Link>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load this ticket.")}</Alert>
        )}

        {isLoading ? (
          <div className="space-y-3">
            <Skeleton className="h-10 w-2/3 rounded-lg" />
            <Skeleton className="h-24 rounded-[18px]" />
            <Skeleton className="h-32 rounded-[18px]" />
          </div>
        ) : ticket ? (
          <>
            <header>
              <h1 className="text-xl font-semibold tracking-tight">{ticket.subject}</h1>
              <p className="mt-1 text-sm text-muted-foreground">
                {ticket.submittedByName} ({ticket.submittedByEmail}) · {categoryLabel[ticket.category]}
              </p>
            </header>

            <div className="grid gap-3 rounded-[18px] border border-border bg-card p-4 shadow-soft sm:grid-cols-3">
              <div className="space-y-1.5">
                <Label htmlFor="triage-status">Status</Label>
                <select
                  id="triage-status"
                  value={ticket.status}
                  onChange={(e) => changeStatus.mutate(e.target.value as TicketStatus)}
                  disabled={changeStatus.isPending}
                  className="h-9 w-full rounded-lg border border-border bg-background px-2 text-sm"
                >
                  {allStatuses.map((option) => (
                    <option key={option} value={option}>
                      {statusLabel[option]}
                    </option>
                  ))}
                </select>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="triage-priority">Priority</Label>
                <select
                  id="triage-priority"
                  value={ticket.priority}
                  onChange={(e) => changePriority.mutate(e.target.value as TicketPriority)}
                  disabled={changePriority.isPending}
                  className="h-9 w-full rounded-lg border border-border bg-background px-2 text-sm"
                >
                  {allPriorities.map((option) => (
                    <option key={option} value={option}>
                      {option}
                    </option>
                  ))}
                </select>
              </div>

              <div className="space-y-1.5">
                <Label>Assigned to</Label>
                <div className="flex items-center gap-2">
                  <Badge variant={ticket.assignedToName ? "outline" : "neutral"} className="flex-1 justify-center py-1.5">
                    {ticket.assignedToName ?? "Unclaimed"}
                  </Badge>
                  {user && (
                    <button
                      type="button"
                      onClick={() => assign.mutate(isMine ? null : user.id)}
                      disabled={assign.isPending}
                      className="rounded-lg border border-border px-2.5 py-1.5 text-xs font-medium hover:bg-muted disabled:opacity-50"
                    >
                      {isMine ? "Unclaim" : "Claim"}
                    </button>
                  )}
                </div>
              </div>
            </div>

            {(changeStatus.isError || changePriority.isError || assign.isError) && (
              <Alert variant="error">
                {getApiErrorMessage(changeStatus.error ?? changePriority.error ?? assign.error)}
              </Alert>
            )}

            <TicketThread messages={ticket.messages} submittedById={ticket.submittedById} />

            <ReplyBox
              onSubmit={(body, isInternalNote) => reply.mutate({ body, isInternalNote })}
              isPending={reply.isPending}
              error={reply.error}
              allowInternalNote
            />
          </>
        ) : null}
      </div>
    </PageTransition>
  );
}
