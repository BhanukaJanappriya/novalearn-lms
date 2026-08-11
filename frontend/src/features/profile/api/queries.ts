import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";

export interface MyProfile {
  id: string;
  fullName: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl: string | null;
  roles: string[];
  joinedAtUtc: string;
}

export const profileKeys = {
  all: ["profile"] as const,
  me: () => [...profileKeys.all, "me"] as const,
};

const profileApi = {
  async me(): Promise<MyProfile> {
    const { data } = await apiClient.get<MyProfile>("/profile/me");
    return data;
  },

  /** Sets the caller's own picture. There is no id: the server uses the token's subject. */
  async setAvatar(avatarUrl: string | null): Promise<MyProfile> {
    const { data } = await apiClient.put<MyProfile>("/profile/me/avatar", { avatarUrl });
    return data;
  },
};

export function useMyProfile(enabled = true) {
  return useQuery({
    queryKey: profileKeys.me(),
    queryFn: () => profileApi.me(),
    enabled,
    staleTime: 60_000,
  });
}

export function useSetMyAvatar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (avatarUrl: string | null) => profileApi.setAvatar(avatarUrl),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: profileKeys.all });
      // The directory and account tables show the same picture, so refresh them too.
      void queryClient.invalidateQueries({ queryKey: ["admin-users"] });
      void queryClient.invalidateQueries({ queryKey: ["directory"] });
    },
  });
}
