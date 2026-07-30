import type { CourseLevel } from "@/features/courses/api/types";
import type { EnrollmentStatus } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const levelVariant: Record<CourseLevel, BadgeVariant> = {
  Beginner: "success",
  Intermediate: "warning",
  Advanced: "destructive",
};

export const statusVariant: Record<EnrollmentStatus, BadgeVariant> = {
  Active: "default",
  Completed: "success",
  Dropped: "neutral",
};

const palettes = [
  ["#8B5CF6", "#A78BFA"],
  ["#2a78d6", "#5598e7"],
  ["#1baf7a", "#3fd39c"],
  ["#eda100", "#f5c451"],
  ["#4a3aa7", "#7a6ad4"],
  ["#e34948", "#f07a79"],
];

/** Deterministic brand-ish gradient from a course code, matching the courses feature. */
export function coverGradient(seed: string): string {
  const hash = [...seed].reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
  const [from, to] = palettes[hash % palettes.length];
  return `linear-gradient(135deg, ${from}, ${to})`;
}

/** Deterministic solid accent from a name, used for roster avatars. */
export function avatarColor(seed: string): string {
  const hash = [...seed].reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
  return palettes[hash % palettes.length][0];
}
