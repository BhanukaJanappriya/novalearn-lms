import { Award, BellRing, ClipboardList, Inbox, ListChecks, type LucideIcon } from "lucide-react";
import type { NotificationType } from "../api/types";

/** Icon and accent per kind, so the feed is scannable without reading every line. */
const visuals: Record<NotificationType, { icon: LucideIcon; accent: string }> = {
  SubmissionGraded: { icon: Award, accent: "text-success" },
  AssignmentPublished: { icon: ClipboardList, accent: "text-primary" },
  QuizPublished: { icon: ListChecks, accent: "text-primary" },
  SubmissionReceived: { icon: Inbox, accent: "text-[hsl(var(--warning))]" },
};

export function notificationVisual(type: NotificationType): { icon: LucideIcon; accent: string } {
  return visuals[type] ?? { icon: BellRing, accent: "text-muted-foreground" };
}
