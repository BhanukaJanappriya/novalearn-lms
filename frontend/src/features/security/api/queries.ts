import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";
import type { LockedAccountRow, PagedResult, SecurityListFilters, SecurityOverview, SessionRow } from "./types";

export const securityKeys = {
  all: ["security"] as const,
  overview: () => [...securityKeys.all, "overview"] as const,
  sessions: (filters: SecurityListFilters) => [...securityKeys.all, "sessions", filters] as const,
  lockedAccounts: (filters: SecurityListFilters) => [...securityKeys.all, "locked-accounts", filters] as const,
};

export function useSecurityOverview() {
  return useQuery({
    queryKey: securityKeys.overview(),
    queryFn: async () => {
      const { data } = await apiClient.get<SecurityOverview>("/admin/security/overview");
      return data;
    },
    staleTime: 15_000,
  });
}

export function useActiveSessions(filters: SecurityListFilters) {
  return useQuery({
    queryKey: securityKeys.sessions(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<SessionRow>>("/admin/security/sessions", {
        params: { search: filters.search || undefined, page: filters.page, pageSize: filters.pageSize },
      });
      return data;
    },
    placeholderData: keepPreviousData,
    staleTime: 10_000,
  });
}

export function useLockedAccounts(filters: SecurityListFilters) {
  return useQuery({
    queryKey: securityKeys.lockedAccounts(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<LockedAccountRow>>("/admin/security/locked-accounts", {
        params: { search: filters.search || undefined, page: filters.page, pageSize: filters.pageSize },
      });
      return data;
    },
    placeholderData: keepPreviousData,
    staleTime: 10_000,
  });
}

function invalidateAll(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: securityKeys.all });
}

/** Forces one session to sign out. */
export function useRevokeSession() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (sessionId: string) => {
      await apiClient.post(`/admin/security/sessions/${sessionId}/revoke`);
    },
    onSuccess: () => invalidateAll(queryClient),
  });
}

/** Signs an account out everywhere. */
export function useRevokeAllSessions() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (userId: string) => {
      await apiClient.post(`/admin/security/users/${userId}/revoke-sessions`);
    },
    onSuccess: () => invalidateAll(queryClient),
  });
}

/** Clears a lockout, letting the account try signing in again immediately. */
export function useUnlockAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (userId: string) => {
      await apiClient.post(`/admin/security/users/${userId}/unlock`);
    },
    onSuccess: () => invalidateAll(queryClient),
  });
}
