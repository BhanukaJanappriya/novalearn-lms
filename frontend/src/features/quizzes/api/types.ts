import type { AssessmentStatus } from "@/features/assessments/api/types";

export type { AssessmentStatus };

export type QuestionType = "MultipleChoice" | "TrueFalse" | "ShortAnswer";

/** Mirrors the backend `QuizSummaryDto`. Carries no question content for either audience. */
export interface QuizSummary {
  id: string;
  courseId: string;
  title: string;
  description: string | null;
  status: AssessmentStatus;
  timeLimitMinutes: number | null;
  maxAttempts: number | null;
  passingScorePercent: number | null;
  shuffleQuestions: boolean;
  questionCount: number;
  totalPoints: number;
  isReadyToPublish: boolean;
  attemptsUsed: number;
  bestScorePercent: number | null;
  hasPassed: boolean;
  canAttempt: boolean;
}

export interface QuizInput {
  title: string;
  description: string | null;
  timeLimitMinutes: number | null;
  maxAttempts: number | null;
  passingScorePercent: number | null;
  shuffleQuestions: boolean;
  status: AssessmentStatus;
}

/** Authoring shapes. These carry the answer key and are only ever fetched by staff. */
export interface AuthoringOption {
  id: string;
  text: string;
  isCorrect: boolean;
  sortOrder: number;
}

export interface AuthoringQuestion {
  id: string;
  text: string;
  type: QuestionType;
  points: number;
  sortOrder: number;
  acceptedAnswers: string[];
  options: AuthoringOption[];
  isAnswerable: boolean;
}

export interface QuizAuthoring {
  quiz: QuizSummary;
  questions: AuthoringQuestion[];
}

export interface SaveQuestionInput {
  questionId?: string;
  text: string;
  type: QuestionType;
  points: number;
  acceptedAnswers: string[];
  options: { text: string; isCorrect: boolean }[];
}

/**
 * Taking shapes. Deliberately have no correctness flag: the server never sends one while an
 * attempt is open, and the client must not invent a place to put one.
 */
export interface TakingOption {
  id: string;
  text: string;
}

export interface TakingQuestion {
  id: string;
  text: string;
  type: QuestionType;
  points: number;
  sortOrder: number;
  options: TakingOption[];
  selectedOptionId: string | null;
  textAnswer: string | null;
}

export interface AttemptInProgress {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  attemptNumber: number;
  startedAtUtc: string;
  deadlineUtc: string | null;
  totalPoints: number;
  questions: TakingQuestion[];
}

/** Result shapes, safe to include answers because the attempt is closed. */
export interface AnswerResult {
  questionId: string;
  questionText: string;
  questionType: QuestionType;
  selectedOptionId: string | null;
  selectedOptionText: string | null;
  textAnswer: string | null;
  correctOptionText: string | null;
  acceptedAnswers: string[];
  isCorrect: boolean;
  pointsAwarded: number;
  pointsPossible: number;
}

export interface AttemptResult {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  studentId: string;
  studentName: string;
  attemptNumber: number;
  startedAtUtc: string;
  submittedAtUtc: string | null;
  pointsAwarded: number;
  totalPoints: number;
  scorePercent: number;
  isPassed: boolean;
  wasLate: boolean;
  passingScorePercent: number | null;
  answers: AnswerResult[];
}

export interface QuizAttemptSummary {
  attemptId: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  attemptNumber: number;
  submittedAtUtc: string | null;
  pointsAwarded: number;
  totalPoints: number;
  scorePercent: number;
  isPassed: boolean;
  wasLate: boolean;
}

export interface QuizResults {
  quizId: string;
  quizTitle: string;
  totalPoints: number;
  passingScorePercent: number | null;
  attemptCount: number;
  distinctLearners: number;
  averageScorePercent: number | null;
  passedCount: number;
  attempts: QuizAttemptSummary[];
}
