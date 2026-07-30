import type { CourseLevel } from "@/features/courses/api/types";

export type EnrollmentStatus = "Active" | "Completed" | "Dropped";

/** Mirrors the backend `PagedResult<T>`. */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/** Mirrors the backend `EnrollmentDto` (carries both the course and student facets). */
export interface Enrollment {
  id: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  courseId: string;
  courseTitle: string;
  courseCode: string;
  courseCategory: string;
  courseLevel: CourseLevel;
  courseCoverImageUrl: string | null;
  status: EnrollmentStatus;
  progressPercent: number;
  enrolledAtUtc: string;
  completedAtUtc: string | null;
}

/** Mirrors the backend `CourseCatalogDto`. */
export interface CatalogCourse {
  id: string;
  title: string;
  code: string;
  description: string | null;
  category: string;
  level: CourseLevel;
  price: number;
  coverImageUrl: string | null;
  lecturerId: string;
  lecturerName: string;
  enrolledCount: number;
  isEnrolled: boolean;
  createdAtUtc: string;
}

/** Query parameters accepted by the catalogue endpoint. */
export interface CatalogFilters {
  search?: string;
  category?: string;
  level?: CourseLevel | "";
  page: number;
  pageSize: number;
}
