import { useState } from "react";
import { Link } from "react-router-dom";
import { AlertTriangle, Inbox, LifeBuoy, Search, UserX } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { PaginationControls } from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { timeAgo } from "@/lib/format";
import { ChartCard } from "@/features/admin/components/charts/ChartCard";
import { useStaffTicketCounts, useStaffTickets } from "../api/queries";
import type { TicketCategory, TicketPriority, TicketStatus } from "../api/types";
import {
  allCategories,
  allPriorities,
  allStatuses,
  categoryLabel,
  priorityVariant,
  statusLabel,
  statusVariant,
} from "../lib/support";

const PAGE_SIZE = 15;

/**
 * The staff queue: every ticket across the platform, with the counts that matter for triage —
 * open, unassigned, and urgent-or-higher — up front rather than buried in a filter.
 */
export function SupportQueuePage() {
  const [status, setStatus] = useState<TicketStatus | "">("");
  const [category, setCategory] = useState<TicketCategory | "">("");
  const [priority, setPriority] = useState<TicketPriority | "">("");
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);
  const [page, setPage] = useState(1);

  const { data: counts } = useStaffTicketCounts();
  const { data: tickets, isLoading, isError, error } = useStaffTickets({
    status: status || undefined,
    category: category || undefined,
    priority: priority || undefined,
    search: debouncedSearch,
    page,
    pageSize: PAGE_SIZE,
  });

  const resetPage = () => setPage(1);

  return (
    <PageTransition>
      <div className="space-y-6">
        <header>
          <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <LifeBuoy className="h-6 w-6 text-primary" aria-hidden />
            Support
          </h1>
          <p className="mt-1 text-muted-foreground">Every ticket raised across the platform.</p>
        </header>

        <div className="grid gap-3 sm:grid-cols-3">
          <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
            <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <Inbox className="h-3.5 w-3.5" aria-hidden />
              Open
            </p>
            <p className="mt-1 text-2xl font-semibold tabular-nums">{counts?.open ?? "—"}</p>
          </div>
          <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
            <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <UserX className="h-3.5 w-3.5" aria-hidden />
              Unassigned
            </p>
            <p className="mt-1 text-2xl font-semibold tabular-nums">{counts?.unassigned ?? "—"}</p>
          </div>
          <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
            <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <AlertTriangle className="h-3.5 w-3.5" aria-hidden />
              High or urgent
            </p>
            <p className="mt-1 text-2xl font-semibold tabular-nums">{counts?.urgentOrHigh ?? "—"}</p>
          </div>
        </div>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load tickets.")}</Alert>
        )}

        <ChartCard title="Tickets" subtitle="Newest activity first">
          <div className="mb-4 flex flex-wrap items-center gap-2">
            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value as TicketStatus | "");
                resetPage();
              }}
              className="h-9 rounded-lg border border-border bg-background px-2 text-xs"
            >
              <option value="">Any status</option>
              {allStatuses.map((option) => (
                <option key={option} value={option}>
                  {statusLabel[option]}
                </option>
              ))}
            </select>

            <select
              value={category}
              onChange={(e) => {
                setCategory(e.target.value as TicketCategory | "");
                resetPage();
              }}
              className="h-9 rounded-lg border border-border bg-background px-2 text-xs"
            >
              <option value="">Any category</option>
              {allCategories.map((option) => (
                <option key={option} value={option}>
                  {categoryLabel[option]}
                </option>
              ))}
            </select>

            <select
              value={priority}
              onChange={(e) => {
                setPriority(e.target.value as TicketPriority | "");
                resetPage();
              }}
              className="h-9 rounded-lg border border-border bg-background px-2 text-xs"
            >
              <option value="">Any priority</option>
              {allPriorities.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>

            <div className="relative ml-auto min-w-[200px] flex-1 sm:flex-initial">
              <Search
                className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
                aria-hidden
              />
              <Input
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value);
                  resetPage();
                }}
                placeholder="Search subject or student"
                aria-label="Search tickets"
                className="h-9 pl-9 text-sm"
              />
            </div>

            {tickets && <Badge variant="neutral">{tickets.totalCount} total</Badge>}
          </div>

          {isLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 6 }).map((_, index) => (
                <Skeleton key={index} className="h-14 rounded-lg" />
              ))}
            </div>
          ) : tickets && tickets.items.length === 0 ? (
            <p className="py-8 text-center text-sm text-muted-foreground">
              No tickets match that search.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[720px] border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border text-left">
                    <th scope="col" className="pb-2 font-medium text-muted-foreground">Subject</th>
                    <th scope="col" className="pb-2 font-medium text-muted-foreground">Student</th>
                    <th scope="col" className="pb-2 font-medium text-muted-foreground">Priority</th>
                    <th scope="col" className="pb-2 font-medium text-muted-foreground">Status</th>
                    <th scope="col" className="pb-2 font-medium text-muted-foreground">Assigned</th>
                    <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Activity</th>
                  </tr>
                </thead>
                <tbody>
                  {tickets?.items.map((ticket) => (
                    <tr key={ticket.id} className="border-b border-border last:border-0 hover:bg-muted/30">
                      <td className="py-3 pr-3">
                        <Link to={`/admin/support/${ticket.id}`} className="font-medium hover:underline">
                          {ticket.subject}
                        </Link>
                      </td>
                      <td className="py-3 pr-3 text-muted-foreground">{ticket.submittedByName}</td>
                      <td className="py-3 pr-3">
                        <Badge variant={priorityVariant[ticket.priority]}>{ticket.priority}</Badge>
                      </td>
                      <td className="py-3 pr-3">
                        <Badge variant={statusVariant[ticket.status]}>{statusLabel[ticket.status]}</Badge>
                      </td>
                      <td className="py-3 pr-3 text-muted-foreground">
                        {ticket.assignedToName ?? <span className="italic">Unclaimed</span>}
                      </td>
                      <td className="py-3 text-right text-xs text-muted-foreground">
                        {timeAgo(ticket.lastActivityAtUtc)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {tickets && (
            <div className="mt-4">
              <PaginationControls
                page={tickets.page}
                totalPages={tickets.totalPages}
                totalCount={tickets.totalCount}
                onPageChange={setPage}
                noun="ticket"
              />
            </div>
          )}
        </ChartCard>
      </div>
    </PageTransition>
  );
}
