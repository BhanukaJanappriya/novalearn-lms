import type { AdminDashboard } from "./types";
import { apiClient } from "@/services/apiClient";

/**
 * Admin data source — now backed by the live .NET endpoint.
 *
 * The response shape matches `AdminDashboard` one-to-one, so no transformation is needed.
 * The API client attaches the bearer token and transparently refreshes it on 401.
 */
export const adminApi = {
  /** @param days Length of the window the enrollment/completion trend charts cover. */
  async getDashboard(days: number): Promise<AdminDashboard> {
    const { data } = await apiClient.get<AdminDashboard>("/admin/dashboard", { params: { days } });
    return data;
  },
};
