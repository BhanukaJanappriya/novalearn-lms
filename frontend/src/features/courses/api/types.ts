export type CourseLevel = "Beginner" | "Intermediate" | "Advanced";
export type CourseStatus = "Draft" | "Published";

/** Mirrors the backend `CourseDto`. */
export interface Course {
  id: string;
  title: string;
  code: string;
  description: string | null;
  category: string;
  level: CourseLevel;
  status: CourseStatus;
  price: number;
  coverImageUrl: string | null;
  lecturerId: string;
  lecturerName: string;
  createdAtUtc: string;
}

export interface CreateCoursePayload {
  title: string;
  code: string;
  description?: string | null;
  category: string;
  level: CourseLevel;
  status: CourseStatus;
  price: number;
  coverImageUrl?: string | null;
}
