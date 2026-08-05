import { apiClient } from "@/services/apiClient";
import type { AppNotification, PagedResult, UnreadCount } from "./types";

export const notificationsApi = {
  async list(unreadOnly = false, page = 1, pageSize = 20): Promise<PagedResult<AppNotification>> {
    const { data } = await apiClient.get<PagedResult<AppNotification>>("/notifications", {
      params: { unreadOnly, page, pageSize },
    });
    return data;
  },

  async unreadCount(): Promise<UnreadCount> {
    const { data } = await apiClient.get<UnreadCount>("/notifications/unread-count");
    return data;
  },

  async markRead(notificationId: string): Promise<void> {
    await apiClient.put(`/notifications/${notificationId}/read`);
  },

  async markAllRead(): Promise<void> {
    await apiClient.post("/notifications/read-all");
  },
};
