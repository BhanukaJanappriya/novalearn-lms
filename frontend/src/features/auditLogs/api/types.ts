import type { PagedResult } from "@/features/enrollments/api/types";

export type { PagedResult };

export type AuditCategory = "UserManagement" | "Courses" | "Departments" | "Finance" | "Settings";

/** Mirrors the backend `AuditLogRow`. */
export interface AuditLogRow {
  id: string;
  category: AuditCategory;
  action: string;
  details: string | null;
  entityType: string | null;
  entityId: string | null;
  actorId: string;
  actorName: string;
  actorEmail: string;
  createdAtUtc: string;
}

/** Query parameters accepted by the audit log endpoint. */
export interface AuditLogFilters {
  category?: AuditCategory;
  search?: string;
  fromUtc?: string;
  toUtc?: string;
  page: number;
  pageSize: number;
}
