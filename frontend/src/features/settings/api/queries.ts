import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";

/** What an anonymous visitor may see: branding, and whether the platform is in maintenance. */
export interface PublicSettings {
  siteName: string;
  supportEmail: string;
  allowNewRegistrations: boolean;
  maintenanceModeEnabled: boolean;
  maintenanceMessage: string | null;
}

/** The full row, for the admin screen. */
export interface PlatformSettings {
  siteName: string;
  supportEmail: string;
  allowNewRegistrations: boolean;
  maintenanceModeEnabled: boolean;
  maintenanceMessage: string | null;
  defaultCurrency: string;
  maxUploadSizeMb: number;
  updatedAtUtc: string | null;
  updatedBy: string | null;
}

export type UpdateSettingsInput = Omit<PlatformSettings, "updatedAtUtc" | "updatedBy">;

export const settingsKeys = {
  all: ["settings"] as const,
  public: ["settings", "public"] as const,
  admin: ["settings", "admin"] as const,
};

/**
 * Branding and maintenance status, safe for anyone to read. Used before sign-in (the auth pages)
 * as well as after, so this lives outside the admin-only settings query.
 */
export function usePublicSettings() {
  return useQuery({
    queryKey: settingsKeys.public,
    queryFn: async () => {
      const { data } = await apiClient.get<PublicSettings>("/settings/public");
      return data;
    },
    // Long-lived and not worth invalidating aggressively: the one thing that must be fresh,
    // maintenance mode, is enforced server-side on every request regardless of what this cache
    // shows — a stale banner is a cosmetic gap, not a security one.
    staleTime: 60_000,
  });
}

export function useSettings() {
  return useQuery({
    queryKey: settingsKeys.admin,
    queryFn: async () => {
      const { data } = await apiClient.get<PlatformSettings>("/admin/settings");
      return data;
    },
  });
}

export function useUpdateSettings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: UpdateSettingsInput) => {
      const { data } = await apiClient.put<PlatformSettings>("/admin/settings", input);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: settingsKeys.admin });
      queryClient.invalidateQueries({ queryKey: settingsKeys.public });
    },
  });
}
