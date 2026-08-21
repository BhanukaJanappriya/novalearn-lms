import { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { RegisterPage } from "@/pages/RegisterPage";
import { VerifyEmailPage } from "@/pages/VerifyEmailPage";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { FullScreenLoader } from "@/components/FullScreenLoader";
import { AdminAreaRoute, HomeRedirect, ProtectedRoute, PublicOnlyRoute, RequireAdmin } from "./ProtectedRoute";

// Code-split the admin area — it (and Recharts) only load for admins/lecturers.
const AdminLayout = lazy(() =>
  import("@/features/admin/layout/AdminLayout").then((m) => ({ default: m.AdminLayout })),
);
const AdminDashboardPage = lazy(() =>
  import("@/features/admin/pages/AdminDashboardPage").then((m) => ({ default: m.AdminDashboardPage })),
);
const CoursesPage = lazy(() =>
  import("@/features/courses/pages/CoursesPage").then((m) => ({ default: m.CoursesPage })),
);
const StudentDashboardPage = lazy(() =>
  import("@/features/student/pages/StudentDashboardPage").then((m) => ({
    default: m.StudentDashboardPage,
  })),
);
const CatalogPage = lazy(() =>
  import("@/features/enrollments/pages/CatalogPage").then((m) => ({ default: m.CatalogPage })),
);
const MyCoursesPage = lazy(() =>
  import("@/features/enrollments/pages/MyCoursesPage").then((m) => ({ default: m.MyCoursesPage })),
);
const CourseRosterPage = lazy(() =>
  import("@/features/enrollments/pages/CourseRosterPage").then((m) => ({ default: m.CourseRosterPage })),
);
const AssignmentsManagerPage = lazy(() =>
  import("@/features/assessments/pages/AssignmentsManagerPage").then((m) => ({
    default: m.AssignmentsManagerPage,
  })),
);
const AssessmentsPage = lazy(() =>
  import("@/features/assessments/pages/AssessmentsPage").then((m) => ({
    default: m.AssessmentsPage,
  })),
);
const GradebookPage = lazy(() =>
  import("@/features/assessments/pages/GradebookPage").then((m) => ({ default: m.GradebookPage })),
);
const MyAssignmentsPage = lazy(() =>
  import("@/features/assessments/pages/MyAssignmentsPage").then((m) => ({
    default: m.MyAssignmentsPage,
  })),
);
const QuizManagerPage = lazy(() =>
  import("@/features/quizzes/pages/QuizManagerPage").then((m) => ({ default: m.QuizManagerPage })),
);
const QuizBuilderPage = lazy(() =>
  import("@/features/quizzes/pages/QuizBuilderPage").then((m) => ({ default: m.QuizBuilderPage })),
);
const QuizResultsPage = lazy(() =>
  import("@/features/quizzes/pages/QuizResultsPage").then((m) => ({ default: m.QuizResultsPage })),
);
const MyQuizzesPage = lazy(() =>
  import("@/features/quizzes/pages/MyQuizzesPage").then((m) => ({ default: m.MyQuizzesPage })),
);
const QuizAttemptPage = lazy(() =>
  import("@/features/quizzes/pages/QuizAttemptPage").then((m) => ({ default: m.QuizAttemptPage })),
);
const AttemptResultPage = lazy(() =>
  import("@/features/quizzes/pages/AttemptResultPage").then((m) => ({ default: m.AttemptResultPage })),
);
const ProfilePage = lazy(() =>
  import("@/features/profile/pages/ProfilePage").then((m) => ({ default: m.ProfilePage })),
);
const DirectoryPage = lazy(() =>
  import("@/features/directory/pages/DirectoryPage").then((m) => ({ default: m.DirectoryPage })),
);
const CheckoutSuccessPage = lazy(() =>
  import("@/features/payments/pages/CheckoutSuccessPage").then((m) => ({
    default: m.CheckoutSuccessPage,
  })),
);
const CheckoutCancelledPage = lazy(() =>
  import("@/features/payments/pages/CheckoutCancelledPage").then((m) => ({
    default: m.CheckoutCancelledPage,
  })),
);
const SupportPage = lazy(() =>
  import("@/features/support/pages/SupportPage").then((m) => ({ default: m.SupportPage })),
);
const TicketDetailPage = lazy(() =>
  import("@/features/support/pages/TicketDetailPage").then((m) => ({ default: m.TicketDetailPage })),
);
const SupportQueuePage = lazy(() =>
  import("@/features/support/pages/SupportQueuePage").then((m) => ({ default: m.SupportQueuePage })),
);
const StaffTicketDetailPage = lazy(() =>
  import("@/features/support/pages/StaffTicketDetailPage").then((m) => ({
    default: m.StaffTicketDetailPage,
  })),
);
const SettingsPage = lazy(() =>
  import("@/features/settings/pages/SettingsPage").then((m) => ({ default: m.SettingsPage })),
);
const FinancePage = lazy(() =>
  import("@/features/finance/pages/FinancePage").then((m) => ({ default: m.FinancePage })),
);
const AnalyticsPage = lazy(() =>
  import("@/features/analytics/pages/AnalyticsPage").then((m) => ({ default: m.AnalyticsPage })),
);
const WallPage = lazy(() =>
  import("@/features/wall/pages/WallPage").then((m) => ({ default: m.WallPage })),
);
const DepartmentsPage = lazy(() =>
  import("@/features/departments/pages/DepartmentsPage").then((m) => ({
    default: m.DepartmentsPage,
  })),
);
const UsersPage = lazy(() =>
  import("@/features/users/pages/UsersPage").then((m) => ({ default: m.UsersPage })),
);
const CourseBuilderPage = lazy(() =>
  import("@/features/content/pages/CourseBuilderPage").then((m) => ({ default: m.CourseBuilderPage })),
);

export function AppRoutes() {
  return (
    <Routes>
      <Route index element={<HomeRedirect />} />

      {/* Verification works whether or not the user is signed in. */}
      <Route path="/verify-email" element={<VerifyEmailPage />} />

      <Route element={<PublicOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route
          path="/dashboard"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <StudentDashboardPage />
            </Suspense>
          }
        />
        <Route
          path="/catalog"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <CatalogPage />
            </Suspense>
          }
        />
        <Route
          path="/my-courses/:courseId/quizzes"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <MyQuizzesPage />
            </Suspense>
          }
        />
        <Route
          path="/my-courses/:courseId/quizzes/:quizId/attempt"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <QuizAttemptPage />
            </Suspense>
          }
        />
        <Route
          path="/my-courses/:courseId/quizzes/:quizId/result/:attemptId"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <AttemptResultPage />
            </Suspense>
          }
        />
        <Route
          path="/my-courses/:courseId/assignments"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <MyAssignmentsPage />
            </Suspense>
          }
        />
        <Route
          path="/profile"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <ProfilePage />
            </Suspense>
          }
        />
        <Route
          path="/checkout/success"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <CheckoutSuccessPage />
            </Suspense>
          }
        />
        <Route
          path="/checkout/cancelled"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <CheckoutCancelledPage />
            </Suspense>
          }
        />
        <Route
          path="/support"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <SupportPage />
            </Suspense>
          }
        />
        <Route
          path="/support/:ticketId"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <TicketDetailPage />
            </Suspense>
          }
        />
        <Route
          path="/wall"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <WallPage />
            </Suspense>
          }
        />
        <Route
          path="/my-courses"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <MyCoursesPage />
            </Suspense>
          }
        />
      </Route>

      <Route element={<AdminAreaRoute />}>
        <Route
          path="/admin"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <AdminLayout />
            </Suspense>
          }
        >
          <Route
            index
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <AdminDashboardPage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="courses"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <CoursesPage />
              </Suspense>
            }
          />
          <Route
            path="content"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <WallPage />
              </Suspense>
            }
          />
          {/* Staff, not admins only: a lecturer sees their own courses' work here. */}
          <Route
            path="assessments"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <AssessmentsPage />
              </Suspense>
            }
          />
          <Route
            path="assignments"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <AssessmentsPage only="assignments" />
              </Suspense>
            }
          />
          <Route
            path="students"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <DirectoryPage audience="students" />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="lecturers"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <DirectoryPage audience="lecturers" />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="analytics"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <AnalyticsPage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="finance"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <FinancePage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="settings"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <SettingsPage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="support"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <SupportQueuePage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="support/:ticketId"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <StaffTicketDetailPage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="departments"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <DepartmentsPage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="users"
            element={
              <RequireAdmin>
                <Suspense fallback={<FullScreenLoader />}>
                  <UsersPage />
                </Suspense>
              </RequireAdmin>
            }
          />
          <Route
            path="courses/:courseId/students"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <CourseRosterPage />
              </Suspense>
            }
          />
          <Route
            path="courses/:courseId/assignments"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <AssignmentsManagerPage />
              </Suspense>
            }
          />
          <Route
            path="courses/:courseId/quizzes"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <QuizManagerPage />
              </Suspense>
            }
          />
          <Route
            path="courses/:courseId/quizzes/:quizId"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <QuizBuilderPage />
              </Suspense>
            }
          />
          <Route
            path="courses/:courseId/quizzes/:quizId/results"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <QuizResultsPage />
              </Suspense>
            }
          />
          <Route
            path="courses/:courseId/gradebook"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <GradebookPage />
              </Suspense>
            }
          />
          <Route
            path="courses/:courseId/content"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <CourseBuilderPage />
              </Suspense>
            }
          />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
