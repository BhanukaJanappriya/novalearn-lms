import { useQuery } from "@tanstack/react-query";
import { adminApi } from "./adminApi";

export const adminKeys = {
  all: ["admin"] as const,
  dashboard: () => [...adminKeys.all, "dashboard"] as const,
};

/**
 * Loads the admin dashboard aggregate. Polling (`refetchInterval`) stands in for
 * the live SignalR push a later slice will add — the UI already treats data as
 * something that refreshes underneath it.
 */
export function useAdminDashboard() {
  return useQuery({
    queryKey: adminKeys.dashboard(),
    queryFn: () => adminApi.getDashboard(),
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}
