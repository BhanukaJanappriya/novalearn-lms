import type { AuditCategory } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const categoryLabel: Record<AuditCategory, string> = {
  UserManagement: "User management",
  Courses: "Courses",
  Departments: "Departments",
  Finance: "Finance",
  Settings: "Settings",
};

export const categoryVariant: Record<AuditCategory, BadgeVariant> = {
  UserManagement: "default",
  Courses: "outline",
  Departments: "neutral",
  Finance: "success",
  Settings: "warning",
};

export const allCategories: AuditCategory[] = [
  "UserManagement",
  "Courses",
  "Departments",
  "Finance",
  "Settings",
];
