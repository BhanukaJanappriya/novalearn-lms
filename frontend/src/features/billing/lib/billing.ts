import type {
  BillingCycle,
  CardBrand,
  InvoiceStatus,
  Plan,
  PlanId,
  SubscriptionStatus,
} from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

/** Formats a whole-unit money amount with its currency symbol. */
export function formatMoney(amount: number, currency: string): string {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: currency.toUpperCase(),
    minimumFractionDigits: Number.isInteger(amount) ? 0 : 2,
  }).format(amount);
}

/** Long, human date: "5 September 2026". */
export function formatDate(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(new Date(iso));
}

/** The price a plan charges per month under the given billing cycle. */
export function monthlyPrice(plan: Plan, cycle: BillingCycle): number {
  return cycle === "annual" ? plan.annualMonthlyPrice : plan.monthlyPrice;
}

/** What annual billing saves versus paying monthly, over a year. */
export function annualSaving(plan: Plan): number {
  return (plan.monthlyPrice - plan.annualMonthlyPrice) * 12;
}

export function findPlan(plans: Plan[], id: PlanId): Plan | undefined {
  return plans.find((plan) => plan.id === id);
}

export const subscriptionStatusLabel: Record<SubscriptionStatus, string> = {
  active: "Active",
  trialing: "Trial",
  past_due: "Past due",
  canceled: "Canceling",
};

export const subscriptionStatusVariant: Record<SubscriptionStatus, BadgeVariant> = {
  active: "success",
  trialing: "default",
  past_due: "destructive",
  canceled: "warning",
};

export const invoiceStatusLabel: Record<InvoiceStatus, string> = {
  paid: "Paid",
  open: "Open",
  void: "Void",
};

export const invoiceStatusVariant: Record<InvoiceStatus, BadgeVariant> = {
  paid: "success",
  open: "warning",
  void: "neutral",
};

export const cardBrandLabel: Record<CardBrand, string> = {
  visa: "Visa",
  mastercard: "Mastercard",
  amex: "American Express",
};
