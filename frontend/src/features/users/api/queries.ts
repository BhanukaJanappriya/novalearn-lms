import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "./usersApi";
import type { UserFilters } from "./types";

export const userKeys = {
  all: ["admin-users"] as const,
  list: (filters: UserFilters) => [...userKeys.all, "list", filters] as const,
  roles: () => [...userKeys.all, "roles"] as const,
};

/** Paged, filtered directory. Previous data is kept so paging does not flash a skeleton. */
export function useUsers(filters: UserFilters) {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => usersApi.list(filters),
    placeholderData: keepPreviousData,
    staleTime: 15_000,
  });
}

/** Assignable role names, served by the API so the client never hardcodes them. */
export function useAssignableRoles() {
  return useQuery({
    queryKey: userKeys.roles(),
    queryFn: () => usersApi.roles(),
    staleTime: 5 * 60_000,
  });
}

export function useSetUserStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, isActive }: { userId: string; isActive: boolean }) =>
      usersApi.setStatus(userId, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.all }),
  });
}

export function useSetUserRoles() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roles }: { userId: string; roles: string[] }) =>
      usersApi.setRoles(userId, roles),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.all }),
  });
}

export function useVerifyUserEmail() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => usersApi.verifyEmail(userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.all }),
  });
}
