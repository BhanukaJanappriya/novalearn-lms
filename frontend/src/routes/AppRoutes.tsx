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
const GradebookPage = lazy(() =>
  import("@/features/assessments/pages/GradebookPage").then((m) => ({ default: m.GradebookPage })),
);
const MyAssignmentsPage = lazy(() =>
  import("@/features/assessments/pages/MyAssignmentsPage").then((m) => ({
    default: m.MyAssignmentsPage,
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
          path="/my-courses/:courseId/assignments"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <MyAssignmentsPage />
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
