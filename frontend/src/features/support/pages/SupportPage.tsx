import { useState } from "react";
import { Link } from "react-router-dom";
import { LifeBuoy, Plus } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { timeAgo } from "@/lib/format";
import { usePublicSettings } from "@/features/settings/api/queries";
import { useMyTickets } from "../api/queries";
import { NewTicketDialog } from "../components/NewTicketDialog";
import { priorityVariant, statusLabel, statusVariant } from "../lib/support";

/** The learner's own support tickets, and a way to raise a new one. */
export function SupportPage() {
  const { data: tickets, isLoading, isError, error } = useMyTickets();
  const { data: platform } = usePublicSettings();
  const [creating, setCreating] = useState(false);

  return (
    <div className="min-h-screen">
      <LearnerHeader />
      <PageTransition>
        <main className="mx-auto max-w-3xl px-6 py-10">
          <header className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
                <LifeBuoy className="h-6 w-6 text-primary" aria-hidden />
                Support
              </h1>
              <p className="mt-1 text-muted-foreground">
                Raise an issue and follow up with our team here.
                {platform?.supportEmail && (
                  <> You can also reach us directly at{" "}
                    <a href={`mailto:${platform.supportEmail}`} className="text-primary hover:underline">
                      {platform.supportEmail}
                    </a>.
                  </>
                )}
              </p>
            </div>
            <button
              type="button"
              onClick={() => setCreating(true)}
              className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground shadow-soft hover:bg-primary/90"
            >
              <Plus className="h-4 w-4" />
              New ticket
            </button>
          </header>

          {isError && (
            <Alert variant="error" className="mt-6">
              {getApiErrorMessage(error, "We could not load your tickets.")}
            </Alert>
          )}

          <div className="mt-6 space-y-3">
            {isLoading ? (
              Array.from({ length: 3 }).map((_, index) => (
                <Skeleton key={index} className="h-20 rounded-[18px]" />
              ))
            ) : tickets && tickets.length === 0 ? (
              <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
                <p className="font-medium">No tickets yet.</p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Something wrong, or a question we can help with? Raise a ticket above.
                </p>
              </div>
            ) : (
              tickets?.map((ticket) => (
                <Link
                  key={ticket.id}
                  to={`/support/${ticket.id}`}
                  className="block rounded-[18px] border border-border bg-card p-4 shadow-soft transition-colors hover:border-primary/30"
                >
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <h2 className="font-medium">{ticket.subject}</h2>
                    <div className="flex shrink-0 gap-1.5">
                      <Badge variant={priorityVariant[ticket.priority]}>{ticket.priority}</Badge>
                      <Badge variant={statusVariant[ticket.status]}>{statusLabel[ticket.status]}</Badge>
                    </div>
                  </div>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {ticket.messageCount} message{ticket.messageCount === 1 ? "" : "s"} · last
                    activity {timeAgo(ticket.lastActivityAtUtc)}
                  </p>
                </Link>
              ))
            )}
          </div>
        </main>
      </PageTransition>

      <NewTicketDialog open={creating} onClose={() => setCreating(false)} />
    </div>
  );
}
