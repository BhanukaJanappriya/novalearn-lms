import type { ReactElement } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { FullScreenLoader } from "@/components/FullScreenLoader";
import { canAccessAdminArea, isAdmin, isLecturer } from "@/lib/roles";

/** Gates authenticated routes; waits for session bootstrap before deciding. */
export function ProtectedRoute() {
  const { isAuthenticated, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}

/** Gates the admin area to admins and lecturers (lecturers get the course tools). */
export function AdminAreaRoute() {
  const { user, isAuthenticated, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return canAccessAdminArea(user) ? <Outlet /> : <Navigate to="/dashboard" replace />;
}

/** Restricts a page within the admin area to admins; lecturers go to courses. */
export function RequireAdmin({ children }: { children: ReactElement }) {
  const { user } = useAuth();
  return isAdmin(user) ? children : <Navigate to="/admin/courses" replace />;
}

/** Routes each role to its natural landing page after sign-in. */
export function HomeRedirect() {
  const { user, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }
  const target = isAdmin(user) ? "/admin" : isLecturer(user) ? "/admin/courses" : "/dashboard";
  return <Navigate to={target} replace />;
}

/** For routes that should be hidden once signed in (login/register). */
export function PublicOnlyRoute() {
  const { isAuthenticated, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }

  return isAuthenticated ? <Navigate to="/" replace /> : <Outlet />;
}
