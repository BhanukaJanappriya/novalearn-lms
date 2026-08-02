export type AssessmentStatus = "Draft" | "Published";

export type SubmissionStatus = "Submitted" | "Graded";

/** Display state of one gradebook cell. */
export type CellStatus = "Missing" | "Submitted" | "Graded";

/** Mirrors the backend `SubmissionDto`. */
export interface Submission {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  maxPoints: number;
  studentId: string;
  studentName: string;
  studentEmail: string;
  content: string;
  attachmentUrl: string | null;
  submittedAtUtc: string;
  isLate: boolean;
  status: SubmissionStatus;
  pointsAwarded: number | null;
  feedback: string | null;
  gradedAtUtc: string | null;
}

/** Mirrors the backend `AssignmentDto`. */
export interface Assignment {
  id: string;
  courseId: string;
  title: string;
  instructions: string | null;
  dueAtUtc: string | null;
  maxPoints: number;
  allowLateSubmissions: boolean;
  status: AssessmentStatus;
  isOpen: boolean;
  submissionCount: number;
  gradedCount: number;
  /** Populated for learners only; staff get the roster instead. */
  mySubmission: Submission | null;
}

export interface AssignmentInput {
  title: string;
  instructions: string | null;
  dueAtUtc: string | null;
  maxPoints: number;
  allowLateSubmissions: boolean;
  status: AssessmentStatus;
}

/** Mirrors the backend gradebook contract. */
export interface GradebookColumn {
  assignmentId: string;
  title: string;
  maxPoints: number;
  dueAtUtc: string | null;
  submittedCount: number;
  gradedCount: number;
  averagePoints: number | null;
}

export interface GradebookCell {
  assignmentId: string;
  submissionId: string | null;
  status: CellStatus;
  pointsAwarded: number | null;
  isLate: boolean;
}

export interface GradebookRow {
  studentId: string;
  studentName: string;
  studentEmail: string;
  cells: GradebookCell[];
  pointsAwarded: number;
  pointsGraded: number;
  percentageOfGraded: number | null;
  missingCount: number;
}

export interface GradebookSummary {
  studentCount: number;
  assignmentCount: number;
  awaitingMarking: number;
  cohortAveragePercentage: number | null;
}

export interface Gradebook {
  courseId: string;
  courseTitle: string;
  courseCode: string;
  totalPointsAvailable: number;
  assignments: GradebookColumn[];
  rows: GradebookRow[];
  summary: GradebookSummary;
}
