import { useState } from "react";
import { Clock, DollarSign, Info, RotateCcw, Search, Wallet } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { PaginationControls } from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { ChartCard, RangeChips } from "@/features/admin/components/charts/ChartCard";
import { TrendMetricCard } from "@/features/analytics/components/TrendMetricCard";
import { useFinanceOverview, useTransactions, type PaymentStatus, type Transaction } from "../api/queries";
import { RefundDialog } from "../components/RefundDialog";
import { RevenueTrendChart } from "../components/RevenueTrendChart";
import { TransactionsTable } from "../components/TransactionsTable";
import { formatMoney, statusFilters, statusLabel } from "../lib/finance";

const RANGES = ["7d", "30d", "90d", "1y"] as const;
type Range = (typeof RANGES)[number];
const rangeDays: Record<Range, number> = { "7d": 7, "30d": 30, "90d": 90, "1y": 365 };

const PAGE_SIZE = 10;

/**
 * The finance ledger: revenue, trend, course breakdown and every transaction, with refunds.
 *
 * The revenue chart and headline are windowed and net of refunds; the course table underneath is
 * lifetime, same distinction platform analytics draws for the same reason — a course's revenue is
 * only meaningful summed over its whole life, not the last thirty days of it.
 */
export function FinancePage() {
  const [range, setRange] = useState<Range>("30d");
  const { data: overview, isLoading: overviewLoading, isError, error } = useFinanceOverview(
    rangeDays[range],
  );

  const [status, setStatus] = useState<PaymentStatus | "">("");
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);
  const [page, setPage] = useState(1);

  const { data: transactions, isLoading: transactionsLoading } = useTransactions({
    status: status || undefined,
    search: debouncedSearch,
    page,
    pageSize: PAGE_SIZE,
  });

  const [refunding, setRefunding] = useState<Transaction | null>(null);

  const currency = overview?.currency ?? "usd";

  return (
    <PageTransition>
      <div className="space-y-6">
        <header className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
              <Wallet className="h-6 w-6 text-primary" aria-hidden />
              Finance
            </h1>
            <p className="mt-1 text-muted-foreground">
              Revenue, transactions and refunds across the platform.
            </p>
          </div>
          <RangeChips options={RANGES} value={range} onChange={setRange} />
        </header>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load finance data.")}</Alert>
        )}

        {overviewLoading ? (
          <>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {Array.from({ length: 4 }).map((_, index) => (
                <Skeleton key={index} className="h-24 rounded-[18px]" />
              ))}
            </div>
            <Skeleton className="h-80 rounded-[18px]" />
          </>
        ) : overview ? (
          <>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              <TrendMetricCard
                label="Net revenue"
                metric={overview.headline.netRevenue}
                icon={DollarSign}
                formatValue={(value) => formatMoney(value, currency)}
              />
              <TrendMetricCard
                label="Transactions"
                metric={overview.headline.transactions}
                icon={Wallet}
              />
              <TrendMetricCard
                label="Refunded"
                metric={overview.headline.refundedAmount}
                icon={RotateCcw}
                formatValue={(value) => formatMoney(value, currency)}
                riseIsGood={false}
              />
              <div className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
                <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
                  <Clock className="h-3.5 w-3.5" aria-hidden />
                  Checkouts in progress
                </p>
                <p className="mt-1 text-2xl font-semibold tabular-nums">
                  {overview.headline.pendingCheckouts}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">Right now, not windowed</p>
              </div>
            </div>

            <ChartCard
              title="Revenue"
              subtitle={`Net of refunds, by ${overview.window.granularity.toLowerCase()}, over the last ${overview.window.days} days`}
            >
              <RevenueTrendChart
                data={overview.series}
                granularity={overview.window.granularity}
                currency={currency}
              />
            </ChartCard>

            <ChartCard title="By course" subtitle="Lifetime figures, highest earning first">
              {overview.courses.length === 0 ? (
                <p className="py-8 text-center text-sm text-muted-foreground">
                  Nothing has been sold yet.
                </p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[520px] border-collapse text-sm">
                    <thead>
                      <tr className="border-b border-border text-left">
                        <th scope="col" className="pb-2 font-medium text-muted-foreground">Course</th>
                        <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Sales</th>
                        <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Gross</th>
                        <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Refunded</th>
                        <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Net</th>
                      </tr>
                    </thead>
                    <tbody>
                      {overview.courses.map((course) => (
                        <tr key={course.courseId} className="border-b border-border last:border-0">
                          <td className="py-3 pr-3 font-medium">{course.courseTitle}</td>
                          <td className="py-3 text-right tabular-nums">{course.transactions}</td>
                          <td className="py-3 text-right tabular-nums">
                            {formatMoney(course.grossRevenue, currency)}
                          </td>
                          <td className="py-3 text-right tabular-nums text-warning">
                            {course.refundedAmount > 0
                              ? `−${formatMoney(course.refundedAmount, currency)}`
                              : "—"}
                          </td>
                          <td className="py-3 text-right font-semibold tabular-nums">
                            {formatMoney(course.netRevenue, currency)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </ChartCard>
          </>
        ) : null}

        <ChartCard title="Transactions" subtitle="Every payment, newest first">
          <div className="mb-4 flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={() => {
                setStatus("");
                setPage(1);
              }}
              aria-pressed={status === ""}
              className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                status === ""
                  ? "bg-primary/10 text-primary"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
              }`}
            >
              All
            </button>
            {statusFilters.map((option) => (
              <button
                key={option}
                type="button"
                onClick={() => {
                  setStatus(option);
                  setPage(1);
                }}
                aria-pressed={status === option}
                className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                  status === option
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {statusLabel[option]}
              </button>
            ))}

            <div className="relative ml-auto min-w-[220px] flex-1 sm:flex-initial">
              <Search
                className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
                aria-hidden
              />
              <Input
                value={search}
                onChange={(event) => {
                  setSearch(event.target.value);
                  setPage(1);
                }}
                placeholder="Search by student or course"
                aria-label="Search transactions"
                className="pl-9"
              />
            </div>

            {transactions && <Badge variant="neutral">{transactions.totalCount} total</Badge>}
          </div>

          {transactionsLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 5 }).map((_, index) => (
                <Skeleton key={index} className="h-12 rounded-lg" />
              ))}
            </div>
          ) : transactions ? (
            <>
              <TransactionsTable transactions={transactions.items} onRefund={setRefunding} />
              <div className="mt-4">
                <PaginationControls
                  page={transactions.page}
                  totalPages={transactions.totalPages}
                  totalCount={transactions.totalCount}
                  onPageChange={setPage}
                  noun="transaction"
                />
              </div>
            </>
          ) : null}
        </ChartCard>

        {!overviewLoading && overview && overview.headline.pendingCheckouts > 0 && (
          <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <Info className="h-3.5 w-3.5 shrink-0" aria-hidden />
            {overview.headline.pendingCheckouts} checkout
            {overview.headline.pendingCheckouts === 1 ? " is" : "s are"} currently in progress and
            not yet reflected in revenue.
          </p>
        )}
      </div>

      <RefundDialog transaction={refunding} onClose={() => setRefunding(null)} />
    </PageTransition>
  );
}
