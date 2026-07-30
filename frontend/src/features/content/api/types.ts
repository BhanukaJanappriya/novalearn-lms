import type { CourseStatus } from "@/features/courses/api/types";

export type LessonType = "Video" | "Pdf" | "Text" | "Link";

/** Mirrors the backend `LessonDto`. */
export interface Lesson {
  id: string;
  moduleId: string;
  title: string;
  type: LessonType;
  contentUrl: string | null;
  textContent: string | null;
  durationMinutes: number | null;
  sortOrder: number;
  isPreview: boolean;
}

/** Mirrors the backend `ModuleDto`. */
export interface CourseModule {
  id: string;
  courseId: string;
  title: string;
  description: string | null;
  sortOrder: number;
  lessons: Lesson[];
}

/** Mirrors the backend `CourseContentDto`. */
export interface CourseContent {
  courseId: string;
  courseTitle: string;
  courseCode: string;
  courseStatus: CourseStatus;
  lecturerId: string;
  modules: CourseModule[];
}

export interface ModulePayload {
  title: string;
  description?: string | null;
}

export interface LessonPayload {
  title: string;
  type: LessonType;
  contentUrl?: string | null;
  textContent?: string | null;
  durationMinutes?: number | null;
  isPreview: boolean;
}
