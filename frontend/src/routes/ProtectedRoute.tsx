import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { FullScreenLoader } from "@/components/FullScreenLoader";
import { isAdmin } from "@/lib/roles";

/** Gates authenticated routes; waits for session bootstrap before deciding. */
export function ProtectedRoute() {
  const { isAuthenticated, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}

/** Gates the admin control center to administrator roles only. */
export function AdminRoute() {
  const { user, isAuthenticated, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return isAdmin(user) ? <Outlet /> : <Navigate to="/dashboard" replace />;
}

/** Sends admins to the control center and everyone else to their dashboard. */
export function HomeRedirect() {
  const { user, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }
  return <Navigate to={isAdmin(user) ? "/admin" : "/dashboard"} replace />;
}

/** For routes that should be hidden once signed in (login/register). */
export function PublicOnlyRoute() {
  const { isAuthenticated, isBootstrapping } = useAuth();

  if (isBootstrapping) {
    return <FullScreenLoader />;
  }

  return isAuthenticated ? <Navigate to="/" replace /> : <Outlet />;
}
