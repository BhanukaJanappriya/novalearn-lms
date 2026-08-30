import { useQuery } from "@tanstack/react-query";
import { adminApi } from "./adminApi";

export const adminKeys = {
  all: ["admin"] as const,
  dashboard: (days: number) => [...adminKeys.all, "dashboard", days] as const,
};

/**
 * Loads the admin dashboard aggregate for a trend window. Polling (`refetchInterval`) stands in
 * for the live SignalR push a later slice will add — the UI already treats data as something
 * that refreshes underneath it.
 */
export function useAdminDashboard(days: number) {
  return useQuery({
    queryKey: adminKeys.dashboard(days),
    queryFn: () => adminApi.getDashboard(days),
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}
