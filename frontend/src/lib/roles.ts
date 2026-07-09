import type { UserSummary } from "@/types/auth";

/** Roles allowed into the admin control center. */
export const ADMIN_ROLES = ["SuperAdministrator", "Administrator"] as const;

export function isAdmin(user: UserSummary | null): boolean {
  if (!user) return false;
  return user.roles.some((role) => ADMIN_ROLES.includes(role as (typeof ADMIN_ROLES)[number]));
}
