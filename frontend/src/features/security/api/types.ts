import type { PagedResult } from "@/features/enrollments/api/types";

export type { PagedResult };

/** Mirrors the backend `SecurityOverview`. */
export interface SecurityOverview {
  activeSessions: number;
  lockedOutAccounts: number;
  failedLoginAttempts: number;
  twoFactorAdoptionPct: number;
}

/** Mirrors the backend `SessionRow`. */
export interface SessionRow {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  createdAtUtc: string;
  expiresAtUtc: string;
  createdByIp: string | null;
}

/** Mirrors the backend `LockedAccountRow`. */
export interface LockedAccountRow {
  userId: string;
  userName: string;
  userEmail: string;
  lockoutEnd: string;
  accessFailedCount: number;
}

export interface SecurityListFilters {
  search?: string;
  page: number;
  pageSize: number;
}
