export type TicketCategory = "Technical" | "Billing" | "Course" | "Account" | "Other";
export type TicketPriority = "Low" | "Normal" | "High" | "Urgent";
export type TicketStatus = "Open" | "InProgress" | "Resolved" | "Closed";

export interface TicketMessage {
  id: string;
  authorId: string;
  authorName: string;
  authorAvatarUrl: string | null;
  body: string;
  isInternalNote: boolean;
  createdAtUtc: string;
}

export interface TicketDetail {
  id: string;
  subject: string;
  category: TicketCategory;
  priority: TicketPriority;
  status: TicketStatus;
  submittedById: string;
  submittedByName: string;
  submittedByEmail: string;
  assignedToId: string | null;
  assignedToName: string | null;
  createdAtUtc: string;
  resolvedAtUtc: string | null;
  closedAtUtc: string | null;
  messages: TicketMessage[];
}

export interface TicketSummary {
  id: string;
  subject: string;
  category: TicketCategory;
  priority: TicketPriority;
  status: TicketStatus;
  submittedById: string;
  submittedByName: string;
  assignedToId: string | null;
  assignedToName: string | null;
  messageCount: number;
  createdAtUtc: string;
  lastActivityAtUtc: string;
}

export interface TicketPage {
  items: TicketSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface StaffTicketCounts {
  open: number;
  unassigned: number;
  urgentOrHigh: number;
}

export interface CreateTicketInput {
  subject: string;
  category: TicketCategory;
  priority: TicketPriority;
  message: string;
}

export interface StaffTicketFilters {
  status?: TicketStatus;
  category?: TicketCategory;
  priority?: TicketPriority;
  assignedToId?: string;
  search?: string;
  page: number;
  pageSize: number;
}
