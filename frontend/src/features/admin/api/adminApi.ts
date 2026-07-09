import type { AdminDashboard } from "./types";
import { mockDashboard } from "./mockData";

/**
 * Admin data source.
 *
 * This is the ONLY file that knows the data is mocked. Every hook/component below
 * depends on the return *types*, not the implementation — so going live means
 * replacing the body of `getDashboard` with a real call, e.g.:
 *
 *   const { data } = await apiClient.get<AdminDashboard>("/admin/dashboard");
 *   return data;
 *
 * ...and nothing in the UI layer changes.
 */

const LATENCY_MS = 650;

function delay<T>(payload: T, ms = LATENCY_MS): Promise<T> {
  return new Promise((resolve) => setTimeout(() => resolve(payload), ms));
}

export const adminApi = {
  getDashboard(): Promise<AdminDashboard> {
    // return apiClient.get<AdminDashboard>("/admin/dashboard").then((r) => r.data);
    return delay(mockDashboard);
  },
};
