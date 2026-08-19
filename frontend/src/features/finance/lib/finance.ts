import type { PaymentStatus } from "../api/queries";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const statusVariant: Record<PaymentStatus, BadgeVariant> = {
  Pending: "neutral",
  Succeeded: "success",
  Failed: "destructive",
  Expired: "neutral",
  PartiallyRefunded: "warning",
  Refunded: "warning",
};

export const statusLabel: Record<PaymentStatus, string> = {
  Pending: "Pending",
  Succeeded: "Paid",
  Failed: "Failed",
  Expired: "Expired",
  PartiallyRefunded: "Partially refunded",
  Refunded: "Refunded",
};

/** Every status a transaction can be filtered by. */
export const statusFilters: PaymentStatus[] = [
  "Succeeded",
  "Pending",
  "PartiallyRefunded",
  "Refunded",
  "Failed",
  "Expired",
];

/** Formats a money amount with the platform's currency symbol. Only usd is supported today. */
export function formatMoney(amount: number, currency: string): string {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: currency.toUpperCase(),
    minimumFractionDigits: 2,
  }).format(amount);
}
