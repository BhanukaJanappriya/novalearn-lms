import type { AssessmentStatus } from "@/features/assessments/api/types";

export type { AssessmentStatus };

export type QuestionType =
  | "MultipleChoice"
  | "TrueFalse"
  | "MultipleResponse"
  | "ShortAnswer"
  | "Essay";

/** Where an attempt sits. PendingReview means essays are still with a marker. */
export type AttemptStatus = "InProgress" | "PendingReview" | "Graded";

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
  hasManuallyMarkedQuestions: boolean;
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
  isRequired: boolean;
  /** Shown to whoever marks an essay. Never sent to a learner. */
  markingGuidance: string | null;
  requiresManualMarking: boolean;
  allowsMultipleSelections: boolean;
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
  isRequired: boolean;
  markingGuidance: string | null;
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
  isRequired: boolean;
  allowsMultipleSelections: boolean;
  /** True when the learner writes prose that a person will mark. */
  isEssay: boolean;
  options: TakingOption[];
  selectedOptionIds: string[];
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
  answerId: string;
  questionId: string;
  questionText: string;
  questionType: QuestionType;
  selectedOptionIds: string[];
  selectedOptionTexts: string[];
  textAnswer: string | null;
  correctOptionTexts: string[];
  acceptedAnswers: string[];
  /** Null while an essay is still unmarked. */
  isCorrect: boolean | null;
  pointsAwarded: number;
  pointsPossible: number;
  requiresManualMarking: boolean;
  isManuallyMarked: boolean;
  isAwaitingMarking: boolean;
  feedback: string | null;
}

export interface AttemptResult {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  studentId: string;
  studentName: string;
  attemptNumber: number;
  status: AttemptStatus;
  startedAtUtc: string;
  submittedAtUtc: string | null;
  markedAtUtc: string | null;
  pointsAwarded: number;
  totalPoints: number;
  scorePercent: number;
  isPassed: boolean;
  wasLate: boolean;
  isAwaitingMarking: boolean;
  awaitingMarkingCount: number;
  passingScorePercent: number | null;
  answers: AnswerResult[];
}

export interface QuizAttemptSummary {
  attemptId: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  attemptNumber: number;
  status: AttemptStatus;
  submittedAtUtc: string | null;
  pointsAwarded: number;
  totalPoints: number;
  scorePercent: number;
  isPassed: boolean;
  wasLate: boolean;
  isAwaitingMarking: boolean;
  awaitingMarkingCount: number;
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
  /** How many attempts still need a person. */
  awaitingReviewCount: number;
  attempts: QuizAttemptSummary[];
}
