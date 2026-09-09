import type { PagedResult } from "@/features/enrollments/api/types";

export type { PagedResult };

/** Mirrors the backend `AdminUserDto`. */
export interface AdminUser {
  id: string;
  fullName: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl: string | null;
  emailConfirmed: boolean;
  isActive: boolean;
  isLockedOut: boolean;
  createdAtUtc: string;
  lastLoginAtUtc: string | null;
  roles: string[];
  enrollmentCount: number;
  coursesOwned: number;
}

/** Body for an administrator adding a new account. */
export interface CreateUserInput {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  password: string;
}

/** Query parameters accepted by the account directory endpoint. */
export interface UserFilters {
  search?: string;
  role?: string;
  isActive?: boolean;
  emailConfirmed?: boolean;
  page: number;
  pageSize: number;
}
