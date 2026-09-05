/**
 * Billing contract.
 *
 * The billing area is presentational today: plan catalogue, the platform's current subscription,
 * its card on file and its past invoices all come from a mock in {@link ./mockData}. Every shape
 * here mirrors what a live billing provider (Stripe, Paddle) would hand back, so swapping the
 * mock for a real fetch in {@link ./queries} is the only change that area will need.
 */

export type BillingCycle = "monthly" | "annual";

export type PlanId = "starter" | "growth" | "scale";

/** One line item in a plan's feature list. Absent lines are shown struck through, not hidden. */
export interface PlanBenefit {
  label: string;
  included: boolean;
  /** Optional one-liner shown under the benefit, e.g. a limit or a caveat. */
  detail?: string;
}

export interface Plan {
  id: PlanId;
  name: string;
  /** One sentence on who the plan is for. */
  tagline: string;
  /** Price per month, in whole currency units, when billed monthly. */
  monthlyPrice: number;
  /** Effective price per month, in whole currency units, when billed annually. */
  annualMonthlyPrice: number;
  currency: string;
  /** Headline entitlement, e.g. "Up to 500 active learners". */
  seatSummary: string;
  /** The plan we steer new customers toward. Exactly one plan sets this. */
  recommended: boolean;
  benefits: PlanBenefit[];
}

export type SubscriptionStatus = "active" | "trialing" | "past_due" | "canceled";

export interface CurrentSubscription {
  planId: PlanId;
  cycle: BillingCycle;
  status: SubscriptionStatus;
  seatsIncluded: number;
  seatsUsed: number;
  /** ISO date the current period ends / the next charge lands. */
  currentPeriodEndsOn: string;
  /** Amount of the next charge, in whole currency units. */
  nextInvoiceAmount: number;
  currency: string;
  /** Set when status is "trialing": ISO date the trial converts to paid. */
  trialEndsOn: string | null;
  /** Set when status is "canceled": ISO date access ends. */
  cancelsOn: string | null;
}

export type CardBrand = "visa" | "mastercard" | "amex";

export interface PaymentMethod {
  brand: CardBrand;
  last4: string;
  expMonth: number;
  expYear: number;
  holderName: string;
  billingEmail: string;
}

export type InvoiceStatus = "paid" | "open" | "void";

export interface Invoice {
  id: string;
  /** Human reference shown to the customer, e.g. "INV-2026-014". */
  number: string;
  /** ISO date the invoice was issued. */
  issuedOn: string;
  description: string;
  amount: number;
  currency: string;
  status: InvoiceStatus;
}

export interface BillingOverview {
  plans: Plan[];
  subscription: CurrentSubscription;
  paymentMethod: PaymentMethod;
  invoices: Invoice[];
}
