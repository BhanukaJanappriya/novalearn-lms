import { FileText, Link2, PlayCircle, FileType2, type LucideIcon } from "lucide-react";
import type { CourseModule, LessonType } from "../api/types";

/** One icon per lesson type, so a learner can scan an outline at a glance. */
export const lessonTypeIcon: Record<LessonType, LucideIcon> = {
  Video: PlayCircle,
  Pdf: FileType2,
  Text: FileText,
  Link: Link2,
};

export const lessonTypeLabel: Record<LessonType, string> = {
  Video: "Video",
  Pdf: "PDF",
  Text: "Text",
  Link: "Link",
};

/** Compact duration, e.g. "45m" or "1h 20m". Returns null when there is no estimate. */
export function formatDuration(minutes: number | null | undefined): string | null {
  if (minutes === null || minutes === undefined || minutes <= 0) return null;
  if (minutes < 60) return `${minutes}m`;

  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  return rest === 0 ? `${hours}h` : `${hours}h ${rest}m`;
}

/** Total minutes across a module's lessons, ignoring lessons with no estimate. */
export function moduleDuration(module: CourseModule): number {
  return module.lessons.reduce((total, lesson) => total + (lesson.durationMinutes ?? 0), 0);
}

export function countLessons(modules: CourseModule[]): number {
  return modules.reduce((total, module) => total + module.lessons.length, 0);
}
