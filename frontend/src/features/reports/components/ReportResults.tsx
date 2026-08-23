import type { UseQueryResult } from "@tanstack/react-query";
import { PaginationControls } from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { ReportTable, type DisplayColumn } from "./ReportTable";

interface ReportResultsProps<T> {
  query: UseQueryResult<T[]>;
  columns: DisplayColumn<T>[];
  rowKey: (row: T) => string;
  emptyMessage: string;
  page: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

/**
 * The preview area shared by every report type: a loading state while it runs, a prompt before
 * the first run, and a paged table once it has rows. Paging is entirely client-side — the backend
 * already returned the whole (capped) result set, per the reasoning in `IReportsRepository`.
 */
export function ReportResults<T>({
  query,
  columns,
  rowKey,
  emptyMessage,
  page,
  pageSize,
  onPageChange,
}: ReportResultsProps<T>) {
  if (query.isFetching && !query.data) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 6 }).map((_, index) => (
          <Skeleton key={index} className="h-12 rounded-lg" />
        ))}
      </div>
    );
  }

  if (!query.data) {
    return (
      <p className="py-8 text-center text-sm text-muted-foreground">
        Choose your filters, then select &quot;Generate report&quot; to run it.
      </p>
    );
  }

  const rows = query.data.slice((page - 1) * pageSize, page * pageSize);
  const totalPages = Math.max(1, Math.ceil(query.data.length / pageSize));

  return (
    <>
      <ReportTable rows={rows} columns={columns} rowKey={rowKey} emptyMessage={emptyMessage} />
      <div className="mt-4">
        <PaginationControls
          page={page}
          totalPages={totalPages}
          totalCount={query.data.length}
          onPageChange={onPageChange}
          noun="row"
        />
      </div>
    </>
  );
}
