import type { ReactNode } from "react";
import type { ReportColumn } from "../lib/csv";

export interface DisplayColumn<T> extends ReportColumn<T> {
  align?: "right";
  /** Overrides how the cell renders; `get` still supplies the CSV export's plain-text value. */
  render?: (row: T) => ReactNode;
}

interface ReportTableProps<T> {
  rows: T[];
  columns: DisplayColumn<T>[];
  rowKey: (row: T) => string;
  emptyMessage: string;
}

/** The one table shell every report type's preview shares; only its columns differ. */
export function ReportTable<T>({ rows, columns, rowKey, emptyMessage }: ReportTableProps<T>) {
  if (rows.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">{emptyMessage}</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[760px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            {columns.map((column) => (
              <th
                key={column.header}
                scope="col"
                className={`pb-2 font-medium text-muted-foreground ${column.align === "right" ? "text-right" : ""}`}
              >
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={rowKey(row)} className="border-b border-border last:border-0 hover:bg-muted/30">
              {columns.map((column) => (
                <td
                  key={column.header}
                  className={`py-3 pr-3 last:pr-0 ${column.align === "right" ? "text-right tabular-nums" : ""}`}
                >
                  {column.render ? column.render(row) : column.get(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
