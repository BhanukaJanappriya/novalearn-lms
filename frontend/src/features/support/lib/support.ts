import type { TicketCategory, TicketPriority, TicketStatus } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const statusVariant: Record<TicketStatus, BadgeVariant> = {
  Open: "warning",
  InProgress: "default",
  Resolved: "success",
  Closed: "neutral",
};

export const statusLabel: Record<TicketStatus, string> = {
  Open: "Open",
  InProgress: "In progress",
  Resolved: "Resolved",
  Closed: "Closed",
};

export const priorityVariant: Record<TicketPriority, BadgeVariant> = {
  Low: "neutral",
  Normal: "outline",
  High: "warning",
  Urgent: "destructive",
};

export const categoryLabel: Record<TicketCategory, string> = {
  Technical: "Technical",
  Billing: "Billing",
  Course: "Course",
  Account: "Account",
  Other: "Other",
};

export const allStatuses: TicketStatus[] = ["Open", "InProgress", "Resolved", "Closed"];
export const allPriorities: TicketPriority[] = ["Low", "Normal", "High", "Urgent"];
export const allCategories: TicketCategory[] = ["Technical", "Billing", "Course", "Account", "Other"];
