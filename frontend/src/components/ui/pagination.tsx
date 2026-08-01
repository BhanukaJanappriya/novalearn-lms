import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "./button";
import { formatNumber } from "@/lib/format";

interface PaginationControlsProps {
  page: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  /** Singular noun for the summary line, pluralised with a trailing "s". */
  noun?: string;
}

/** Previous/next paging with a "page x of y" summary. Renders nothing for a single page. */
export function PaginationControls({
  page,
  totalPages,
  totalCount,
  onPageChange,
  noun = "item",
}: PaginationControlsProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 border-t border-border pt-4">
      <p className="text-sm text-muted-foreground">
        Page {page} of {totalPages} · {formatNumber(totalCount)} {noun}
        {totalCount === 1 ? "" : "s"}
      </p>
      <div className="flex items-center gap-2">
        <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          <ChevronLeft className="h-4 w-4" />
          Previous
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Next
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
