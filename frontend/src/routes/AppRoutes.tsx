import { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { RegisterPage } from "@/pages/RegisterPage";
import { VerifyEmailPage } from "@/pages/VerifyEmailPage";
import { DashboardPage } from "@/pages/DashboardPage";
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
const CatalogPage = lazy(() =>
  import("@/features/enrollments/pages/CatalogPage").then((m) => ({ default: m.CatalogPage })),
);
const MyCoursesPage = lazy(() =>
  import("@/features/enrollments/pages/MyCoursesPage").then((m) => ({ default: m.MyCoursesPage })),
);
const CourseRosterPage = lazy(() =>
  import("@/features/enrollments/pages/CourseRosterPage").then((m) => ({ default: m.CourseRosterPage })),
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
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route
          path="/catalog"
          element={
            <Suspense fallback={<FullScreenLoader />}>
              <CatalogPage />
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
            path="courses/:courseId/students"
            element={
              <Suspense fallback={<FullScreenLoader />}>
                <CourseRosterPage />
              </Suspense>
            }
          />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
