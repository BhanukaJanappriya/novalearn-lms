const numberFormat = new Intl.NumberFormat("en-US");

/** Formats a trust-strip figure with thousands separators, e.g. 12000 becomes "12,000". */
export function formatCount(value: number): string {
  return numberFormat.format(value);
}
