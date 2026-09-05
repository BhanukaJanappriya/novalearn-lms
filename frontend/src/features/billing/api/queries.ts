import { useQuery } from "@tanstack/react-query";
import type { BillingOverview } from "./types";
import { billingOverview } from "./mockData";

export const billingKeys = {
  all: ["billing"] as const,
  overview: () => [...billingKeys.all, "overview"] as const,
};

/**
 * The billing overview: plan catalogue, current subscription, card on file and past invoices.
 *
 * Resolves from a local mock today. When a billing provider is wired up, only the `queryFn`
 * changes — every consumer already treats this as data that arrives asynchronously and can fail.
 */
export function useBillingOverview() {
  return useQuery({
    queryKey: billingKeys.overview(),
    queryFn: async (): Promise<BillingOverview> => billingOverview,
    staleTime: 60_000,
  });
}
