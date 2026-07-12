import type { UserSummary } from "@/types/auth";

/** Roles allowed into the admin control center. */
export const ADMIN_ROLES = ["SuperAdministrator", "Administrator"] as const;

export function isAdmin(user: UserSummary | null): boolean {
  if (!user) return false;
  return user.roles.some((role) => ADMIN_ROLES.includes(role as (typeof ADMIN_ROLES)[number]));
}

export function isLecturer(user: UserSummary | null): boolean {
  return user?.roles.includes("Lecturer") ?? false;
}

/** Who may open the admin area (admins land on the dashboard, lecturers on courses). */
export function canAccessAdminArea(user: UserSummary | null): boolean {
  return isAdmin(user) || isLecturer(user);
}

/** Who may create courses and delete their own (admins may delete any). */
export function canManageCourses(user: UserSummary | null): boolean {
  return isAdmin(user) || isLecturer(user);
}
