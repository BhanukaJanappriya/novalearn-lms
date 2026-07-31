import type { CourseLevel } from "@/features/courses/api/types";
import type { EnrollmentStatus } from "@/features/enrollments/api/types";

/** Mirrors the backend `StudentSummaryDto`. */
export interface StudentSummary {
  activeCourses: number;
  completedCourses: number;
  averageProgressPercent: number;
  lessonsAvailable: number;
  learningMinutes: number;
  coursesNearlyDone: number;
}

/** Mirrors the backend `StudentCourseDto`. */
export interface StudentCourse {
  enrollmentId: string;
  courseId: string;
  title: string;
  code: string;
  category: string;
  level: CourseLevel;
  coverImageUrl: string | null;
  lecturerName: string;
  status: EnrollmentStatus;
  progressPercent: number;
  moduleCount: number;
  lessonCount: number;
  totalMinutes: number;
  firstLessonTitle: string | null;
  enrolledAtUtc: string;
  completedAtUtc: string | null;
}

/** Mirrors the backend `CategoryProgressDto`. */
export interface CategoryProgress {
  label: string;
  courseCount: number;
  averageProgressPercent: number;
}

/** Mirrors the backend `ActivityPointDto`. */
export interface ActivityPoint {
  label: string;
  value: number;
}

/** Mirrors the backend `RecommendedCourseDto`. */
export interface RecommendedCourse {
  courseId: string;
  title: string;
  code: string;
  category: string;
  level: CourseLevel;
  price: number;
  coverImageUrl: string | null;
  lecturerName: string;
  enrolledCount: number;
  lessonCount: number;
}

/** Mirrors the backend `StudentDashboardResponse`. */
export interface StudentDashboard {
  summary: StudentSummary;
  continueLearning: StudentCourse[];
  completed: StudentCourse[];
  categoryProgress: CategoryProgress[];
  enrollmentActivity: ActivityPoint[];
  recommended: RecommendedCourse[];
}
