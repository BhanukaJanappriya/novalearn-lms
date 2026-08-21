import { Link, useParams } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { useReplyToTicket, useTicket } from "../api/queries";
import { ReplyBox } from "../components/ReplyBox";
import { TicketThread } from "../components/TicketThread";
import { categoryLabel, priorityVariant, statusLabel, statusVariant } from "../lib/support";

/** The submitter's own view of a ticket: the thread, and a way to reply. */
export function TicketDetailPage() {
  const { ticketId } = useParams<{ ticketId: string }>();
  const { data: ticket, isLoading, isError, error } = useTicket(ticketId);
  const reply = useReplyToTicket(ticketId ?? "");

  return (
    <div className="min-h-screen">
      <LearnerHeader />
      <PageTransition>
        <main className="mx-auto max-w-3xl px-6 py-10">
          <Link
            to="/support"
            className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="h-4 w-4" aria-hidden />
            Back to support
          </Link>

          {isError && (
            <Alert variant="error" className="mt-4">
              {getApiErrorMessage(error, "We could not load this ticket.")}
            </Alert>
          )}

          {isLoading ? (
            <div className="mt-4 space-y-3">
              <Skeleton className="h-10 w-2/3 rounded-lg" />
              <Skeleton className="h-32 rounded-[18px]" />
              <Skeleton className="h-32 rounded-[18px]" />
            </div>
          ) : ticket ? (
            <>
              <header className="mt-4">
                <div className="flex flex-wrap items-center gap-2">
                  <h1 className="text-xl font-semibold tracking-tight">{ticket.subject}</h1>
                  <Badge variant={statusVariant[ticket.status]}>{statusLabel[ticket.status]}</Badge>
                  <Badge variant={priorityVariant[ticket.priority]}>{ticket.priority}</Badge>
                </div>
                <p className="mt-1 text-xs text-muted-foreground">
                  {categoryLabel[ticket.category]}
                  {ticket.assignedToName && <> · being handled by {ticket.assignedToName}</>}
                </p>
              </header>

              <div className="mt-6">
                <TicketThread messages={ticket.messages} submittedById={ticket.submittedById} />
              </div>

              <div className="mt-6">
                <ReplyBox
                  onSubmit={(body) => reply.mutate({ body })}
                  isPending={reply.isPending}
                  error={reply.error}
                  disabled={ticket.status === "Closed"}
                  disabledReason="This ticket is closed. Raise a new ticket if the issue is still happening."
                />
              </div>
            </>
          ) : null}
        </main>
      </PageTransition>
    </div>
  );
}
