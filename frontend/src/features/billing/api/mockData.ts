import type { BillingOverview, Plan } from "./types";

/**
 * Stand-in billing data. A live provider would return the same shapes; until that integration
 * lands, every number here is fixed so the page renders identically on each visit.
 */

const CURRENCY = "usd";

export const plans: Plan[] = [
  {
    id: "starter",
    name: "Starter",
    tagline: "For a single department piloting online delivery.",
    monthlyPrice: 149,
    annualMonthlyPrice: 119,
    currency: CURRENCY,
    seatSummary: "Up to 150 active learners",
    recommended: false,
    benefits: [
      { label: "150 active learner seats", included: true },
      { label: "Unlimited courses and lessons", included: true },
      { label: "Quizzes, assignments and a gradebook", included: true },
      { label: "Email and knowledge-base support", included: true, detail: "Next business day" },
      { label: "Standard reports", included: true, detail: "Enrolment, completion, revenue" },
      { label: "Custom domain and branding", included: false },
      { label: "SSO and SCIM provisioning", included: false },
      { label: "Dedicated success manager", included: false },
    ],
  },
  {
    id: "growth",
    name: "Growth",
    tagline: "For an institution running programmes across faculties.",
    monthlyPrice: 399,
    annualMonthlyPrice: 319,
    currency: CURRENCY,
    seatSummary: "Up to 750 active learners",
    recommended: true,
    benefits: [
      { label: "750 active learner seats", included: true },
      { label: "Everything in Starter", included: true },
      { label: "Custom domain and branding", included: true },
      { label: "Live cohort analytics and trends", included: true },
      { label: "Priority support", included: true, detail: "4-hour response, 24/5" },
      { label: "Bulk enrolment and CSV imports", included: true },
      { label: "SSO and SCIM provisioning", included: false },
      { label: "Dedicated success manager", included: false },
    ],
  },
  {
    id: "scale",
    name: "Scale",
    tagline: "For a multi-campus organisation with compliance needs.",
    monthlyPrice: 899,
    annualMonthlyPrice: 749,
    currency: CURRENCY,
    seatSummary: "Unlimited active learners",
    recommended: false,
    benefits: [
      { label: "Unlimited active learner seats", included: true },
      { label: "Everything in Growth", included: true },
      { label: "SSO and SCIM provisioning", included: true, detail: "SAML, OIDC, Okta, Entra ID" },
      { label: "Audit log export and data residency", included: true },
      { label: "99.9% uptime SLA", included: true },
      { label: "Dedicated success manager", included: true },
      { label: "Custom contract and invoicing", included: true },
      { label: "Onboarding and migration assistance", included: true },
    ],
  },
];

export const billingOverview: BillingOverview = {
  plans,
  subscription: {
    planId: "growth",
    cycle: "annual",
    status: "active",
    seatsIncluded: 750,
    seatsUsed: 508,
    currentPeriodEndsOn: "2027-03-01",
    nextInvoiceAmount: 319 * 12,
    currency: CURRENCY,
    trialEndsOn: null,
    cancelsOn: null,
  },
  paymentMethod: {
    brand: "visa",
    last4: "4242",
    expMonth: 11,
    expYear: 2028,
    holderName: "NovaLearn Institute",
    billingEmail: "accounts@novalearn.edu",
  },
  invoices: [
    {
      id: "in_0014",
      number: "INV-2026-014",
      issuedOn: "2026-03-01",
      description: "Growth plan, annual, 750 seats",
      amount: 319 * 12,
      currency: CURRENCY,
      status: "paid",
    },
    {
      id: "in_0009",
      number: "INV-2025-009",
      issuedOn: "2025-03-01",
      description: "Growth plan, annual, 500 seats",
      amount: 299 * 12,
      currency: CURRENCY,
      status: "paid",
    },
    {
      id: "in_0004",
      number: "INV-2024-004",
      issuedOn: "2024-08-01",
      description: "Starter plan, annual, 150 seats",
      amount: 119 * 12,
      currency: CURRENCY,
      status: "paid",
    },
    {
      id: "in_0002",
      number: "INV-2024-002",
      issuedOn: "2024-03-01",
      description: "Starter plan prorated upgrade",
      amount: 92,
      currency: CURRENCY,
      status: "void",
    },
  ],
};
