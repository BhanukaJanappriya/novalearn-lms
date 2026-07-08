import { useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { authApi } from "@/services/authApi";
import type { UserSummary } from "@/types/auth";
import { AuthContext, type AuthContextValue } from "./AuthContext";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserSummary | null>(null);
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  const setSession = useCallback((nextUser: UserSummary) => setUser(nextUser), []);
  const clearSession = useCallback(() => setUser(null), []);

  // On first load, try to restore a session from the httpOnly refresh cookie.
  useEffect(() => {
    let cancelled = false;
    void (async () => {
      const session = await authApi.refresh();
      if (!cancelled) {
        setUser(session?.user ?? null);
        setIsBootstrapping(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      isBootstrapping,
      setSession,
      clearSession,
    }),
    [user, isBootstrapping, setSession, clearSession],
  );

  return <AuthContext value={value}>{children}</AuthContext>;
}
