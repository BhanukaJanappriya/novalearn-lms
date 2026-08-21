import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";
import type {
  CreateTicketInput,
  StaffTicketCounts,
  StaffTicketFilters,
  TicketDetail,
  TicketPage,
  TicketPriority,
  TicketStatus,
  TicketSummary,
} from "./types";

export const supportKeys = {
  all: ["support"] as const,
  mine: ["support", "mine"] as const,
  ticket: (id: string) => ["support", "ticket", id] as const,
  staffTickets: (filters: StaffTicketFilters) => ["support", "staff", filters] as const,
  staffCounts: ["support", "staff", "counts"] as const,
};

/** The caller's own tickets. Shared by whoever is signed in — staff submit tickets too. */
export function useMyTickets() {
  return useQuery({
    queryKey: supportKeys.mine,
    queryFn: async () => {
      const { data } = await apiClient.get<TicketSummary[]>("/support/tickets");
      return data;
    },
    staleTime: 15_000,
  });
}

/** A single ticket. Works for both the submitter's own view and staff's — the server decides
 * which fields (internal notes) come back based on who is asking. */
export function useTicket(id: string | undefined, options?: { refetchInterval?: number | false }) {
  return useQuery({
    queryKey: supportKeys.ticket(id ?? ""),
    queryFn: async () => {
      const { data } = await apiClient.get<TicketDetail>(`/support/tickets/${id}`);
      return data;
    },
    enabled: Boolean(id),
    refetchInterval: options?.refetchInterval,
  });
}

export function useCreateTicket() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: CreateTicketInput) => {
      const { data } = await apiClient.post<TicketDetail>("/support/tickets", input);
      return data;
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: supportKeys.mine }),
  });
}

export function useReplyToTicket(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ body, isInternalNote = false }: { body: string; isInternalNote?: boolean }) => {
      const { data } = await apiClient.post<TicketDetail>(`/support/tickets/${id}/replies`, {
        body,
        isInternalNote,
      });
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: supportKeys.ticket(id) });
      queryClient.invalidateQueries({ queryKey: supportKeys.mine });
      queryClient.invalidateQueries({ queryKey: supportKeys.all });
    },
  });
}

// --- Staff side ---

export function useStaffTickets(filters: StaffTicketFilters) {
  return useQuery({
    queryKey: supportKeys.staffTickets(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<TicketPage>("/admin/support/tickets", {
        params: {
          status: filters.status || undefined,
          category: filters.category || undefined,
          priority: filters.priority || undefined,
          assignedToId: filters.assignedToId || undefined,
          search: filters.search || undefined,
          page: filters.page,
          pageSize: filters.pageSize,
        },
      });
      return data;
    },
    staleTime: 10_000,
  });
}

export function useStaffTicketCounts() {
  return useQuery({
    queryKey: supportKeys.staffCounts,
    queryFn: async () => {
      const { data } = await apiClient.get<StaffTicketCounts>("/admin/support/tickets/counts");
      return data;
    },
    staleTime: 10_000,
  });
}

function invalidateEverything(queryClient: ReturnType<typeof useQueryClient>, id: string) {
  queryClient.invalidateQueries({ queryKey: supportKeys.ticket(id) });
  queryClient.invalidateQueries({ queryKey: supportKeys.all });
}

export function useAssignTicket(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (assignedToId: string | null) => {
      const { data } = await apiClient.put<TicketDetail>(`/admin/support/tickets/${id}/assignment`, {
        assignedToId,
      });
      return data;
    },
    onSuccess: () => invalidateEverything(queryClient, id),
  });
}

export function useChangeTicketStatus(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (status: TicketStatus) => {
      const { data } = await apiClient.put<TicketDetail>(`/admin/support/tickets/${id}/status`, { status });
      return data;
    },
    onSuccess: () => invalidateEverything(queryClient, id),
  });
}

export function useChangeTicketPriority(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (priority: TicketPriority) => {
      const { data } = await apiClient.put<TicketDetail>(`/admin/support/tickets/${id}/priority`, {
        priority,
      });
      return data;
    },
    onSuccess: () => invalidateEverything(queryClient, id),
  });
}
