import { useMutation } from "@tanstack/react-query";
import { NavLink, useNavigate } from "react-router-dom";
import { LogOut } from "lucide-react";
import { Logo } from "@/components/Logo";
import { ThemeToggle } from "@/components/ThemeToggle";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/context/AuthContext";
import { authApi } from "@/services/authApi";
import { cn } from "@/lib/utils";

const links = [
  { to: "/dashboard", label: "Dashboard" },
  { to: "/catalog", label: "Catalog" },
  { to: "/my-courses", label: "My courses" },
  { to: "/wall", label: "Wall" },
  { to: "/support", label: "Support" },
  { to: "/profile", label: "Profile" },
];

/**
 * Shared chrome for the learner-facing pages: brand, section links, theme toggle
 * and sign-out. Kept identical to the original dashboard header so the pages feel
 * like one product.
 */
export function LearnerHeader() {
  const { clearSession } = useAuth();
  const navigate = useNavigate();

  const logout = useMutation({
    mutationFn: () => authApi.logout(),
    onSettled: () => {
      clearSession();
      navigate("/login", { replace: true });
    },
  });

  return (
    <header className="border-b border-border bg-card">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-6 py-3">
        <div className="flex items-center gap-6">
          <Logo />
          <nav className="flex items-center gap-1">
            {links.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  cn(
                    "rounded-lg px-3 py-1.5 text-sm font-medium transition-colors",
                    isActive
                      ? "bg-primary/10 text-primary"
                      : "text-muted-foreground hover:bg-muted hover:text-foreground",
                  )
                }
              >
                {link.label}
              </NavLink>
            ))}
          </nav>
        </div>
        <div className="flex items-center gap-2">
          <NotificationBell />
          <ThemeToggle />
          <Button variant="outline" size="sm" onClick={() => logout.mutate()} isLoading={logout.isPending}>
            <LogOut className="h-4 w-4" />
            Sign out
          </Button>
        </div>
      </div>
    </header>
  );
}
