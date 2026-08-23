/** A column an export can render as text. Table components may extend this with JSX rendering. */
export interface ReportColumn<T> {
  header: string;
  get: (row: T) => string | number | boolean | null;
}

function escapeCsvValue(value: string | number | boolean | null): string {
  if (value === null || value === undefined) {
    return "";
  }
  const text = String(value);
  return /[",\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

export function toCsv<T>(rows: T[], columns: ReportColumn<T>[]): string {
  const header = columns.map((column) => escapeCsvValue(column.header)).join(",");
  const lines = rows.map((row) => columns.map((column) => escapeCsvValue(column.get(row))).join(","));
  return [header, ...lines].join("\r\n");
}

/** Triggers a browser download of a CSV string. A UTF-8 BOM is prepended so Excel opens it correctly. */
export function downloadCsv(filename: string, csv: string): void {
  const blob = new Blob([`﻿${csv}`], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
