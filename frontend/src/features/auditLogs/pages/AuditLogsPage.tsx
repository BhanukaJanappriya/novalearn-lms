import { useEffect, useMemo, useState } from "react";
import { ScrollText, Search } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { PaginationControls } from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { timeAgo } from "@/lib/format";
import { useAuditLogs } from "../api/queries";
import type { AuditCategory, AuditLogFilters } from "../api/types";
import { allCategories, categoryLabel, categoryVariant } from "../lib/auditLogs";

const PAGE_SIZE = 20;

function toIsoOrUndefined(date: string, endOfDay = false): string | undefined {
  if (!date) return undefined;
  return new Date(`${date}T${endOfDay ? "23:59:59" : "00:00:00"}`).toISOString();
}

/**
 * The platform's audit trail: a curated set of the most sensitive admin actions, newest first.
 * Not every command in the app logs here — role and status changes, course and department
 * deletion, settings edits and refunds do, because those are the actions an admin actually needs
 * to be able to answer "who did this and when" about.
 */
export function AuditLogsPage() {
  const [category, setCategory] = useState<AuditCategory | "">("");
  const [search, setSearch] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [page, setPage] = useState(1);

  const debouncedSearch = useDebouncedValue(search, 300);

  useEffect(() => setPage(1), [category, debouncedSearch, fromDate, toDate]);

  const filters: AuditLogFilters = useMemo(
    () => ({
      category: category || undefined,
      search: debouncedSearch || undefined,
      fromUtc: toIsoOrUndefined(fromDate),
      toUtc: toIsoOrUndefined(toDate, true),
      page,
      pageSize: PAGE_SIZE,
    }),
    [category, debouncedSearch, fromDate, toDate, page],
  );

  const { data, isLoading, isError, error } = useAuditLogs(filters);

  return (
    <PageTransition>
      <div className="space-y-6">
        <header>
          <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <ScrollText className="h-6 w-6 text-primary" aria-hidden />
            Audit Logs
          </h1>
          <p className="mt-1 text-muted-foreground">
            Who did what, and when, across the platform's most sensitive actions.
          </p>
        </header>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load the audit log.")}</Alert>
        )}

        <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
          <div className="flex flex-wrap gap-1.5">
            <button
              type="button"
              onClick={() => setCategory("")}
              aria-pressed={category === ""}
              className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                category === ""
                  ? "bg-primary/10 text-primary"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
              }`}
            >
              All
            </button>
            {allCategories.map((option) => (
              <button
                key={option}
                type="button"
                onClick={() => setCategory(option)}
                aria-pressed={category === option}
                className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                  category === option
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {categoryLabel[option]}
              </button>
            ))}
          </div>

          <div className="mt-3 flex flex-wrap items-end gap-3">
            <div className="relative min-w-[220px] flex-1 space-y-1.5">
              <Label htmlFor="audit-search">Search</Label>
              <div className="relative">
                <Search
                  className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
                  aria-hidden
                />
                <Input
                  id="audit-search"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder="Action, details or admin"
                  className="h-9 pl-9"
                />
              </div>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="audit-from">From</Label>
              <Input
                id="audit-from"
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                className="h-9"
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="audit-to">To</Label>
              <Input
                id="audit-to"
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                className="h-9"
              />
            </div>
            {data && <Badge variant="neutral">{data.totalCount} total</Badge>}
          </div>
        </div>

        {isLoading ? (
          <div className="space-y-2">
            {Array.from({ length: 8 }).map((_, index) => (
              <Skeleton key={index} className="h-14 rounded-lg" />
            ))}
          </div>
        ) : data && data.items.length === 0 ? (
          <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">No audit entries match these filters.</p>
            <p className="mt-1 text-sm text-muted-foreground">Try clearing the search or category.</p>
          </div>
        ) : data ? (
          <div className="rounded-[18px] border border-border bg-card shadow-soft">
            <div className="overflow-x-auto">
              <table className="w-full min-w-[820px] border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border text-left">
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Category</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Action</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Details</th>
                    <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Admin</th>
                    <th scope="col" className="px-4 py-3 text-right font-medium text-muted-foreground">When</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((entry) => (
                    <tr key={entry.id} className="border-b border-border last:border-0 hover:bg-muted/30">
                      <td className="px-4 py-3">
                        <Badge variant={categoryVariant[entry.category]}>{categoryLabel[entry.category]}</Badge>
                      </td>
                      <td className="px-4 py-3 font-medium">{entry.action}</td>
                      <td className="max-w-[280px] truncate px-4 py-3 text-muted-foreground" title={entry.details ?? undefined}>
                        {entry.details ?? "—"}
                      </td>
                      <td className="px-4 py-3">
                        <span className="block">{entry.actorName}</span>
                        <span className="block text-xs text-muted-foreground">{entry.actorEmail}</span>
                      </td>
                      <td className="px-4 py-3 text-right text-xs text-muted-foreground">
                        {timeAgo(entry.createdAtUtc)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="p-4 pt-0">
              <PaginationControls
                page={data.page}
                totalPages={data.totalPages}
                totalCount={data.totalCount}
                onPageChange={setPage}
                noun="entry"
              />
            </div>
          </div>
        ) : null}
      </div>
    </PageTransition>
  );
}
