import { createContext, use } from "react";
import type { UserSummary } from "@/types/auth";

export interface AuthContextValue {
  user: UserSummary | null;
  isAuthenticated: boolean;
  isBootstrapping: boolean;
  setSession: (user: UserSummary) => void;
  clearSession: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

/** Access the auth session. Must be used within <AuthProvider>. */
export function useAuth(): AuthContextValue {
  const context = use(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }
  return context;
}
