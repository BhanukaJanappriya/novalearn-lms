import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";
import type { AuditLogFilters, AuditLogRow, PagedResult } from "./types";

export const auditLogKeys = {
  all: ["audit-logs"] as const,
  list: (filters: AuditLogFilters) => [...auditLogKeys.all, filters] as const,
};

export function useAuditLogs(filters: AuditLogFilters) {
  return useQuery({
    queryKey: auditLogKeys.list(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<PagedResult<AuditLogRow>>("/admin/audit-logs", {
        params: {
          category: filters.category || undefined,
          search: filters.search || undefined,
          fromUtc: filters.fromUtc || undefined,
          toUtc: filters.toUtc || undefined,
          page: filters.page,
          pageSize: filters.pageSize,
        },
      });
      return data;
    },
    staleTime: 10_000,
  });
}
