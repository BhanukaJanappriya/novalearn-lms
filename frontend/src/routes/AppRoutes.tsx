import { lazy, Suspense } from "react";
import { Route, Routes } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { RegisterPage } from "@/pages/RegisterPage";
import { VerifyEmailPage } from "@/pages/VerifyEmailPage";
import { DashboardPage } from "@/pages/DashboardPage";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { FullScreenLoader } from "@/components/FullScreenLoader";
import { AdminRoute, HomeRedirect, ProtectedRoute, PublicOnlyRoute } from "./ProtectedRoute";

// Code-split the admin control center — it (and Recharts) only load for admins.
const AdminLayout = lazy(() =>
  import("@/features/admin/layout/AdminLayout").then((m) => ({ default: m.AdminLayout })),
);
const AdminDashboardPage = lazy(() =>
  import("@/features/admin/pages/AdminDashboardPage").then((m) => ({ default: m.AdminDashboardPage })),
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
      </Route>

      <Route element={<AdminRoute />}>
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
              <Suspense fallback={<FullScreenLoader />}>
                <AdminDashboardPage />
              </Suspense>
            }
          />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
