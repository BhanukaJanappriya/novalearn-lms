import type { PagedResult } from "@/features/enrollments/api/types";

export type { PagedResult };

export type NotificationType =
  | "SubmissionGraded"
  | "AssignmentPublished"
  | "QuizPublished"
  | "SubmissionReceived";

/** Mirrors the backend `NotificationDto`. Sent by both the REST endpoint and the live push. */
export interface AppNotification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  link: string | null;
  isRead: boolean;
  createdAtUtc: string;
  readAtUtc: string | null;
}

export interface UnreadCount {
  unreadCount: number;
}
