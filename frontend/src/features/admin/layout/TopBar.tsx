import { useState } from "react";
import {
  Bell,
  CircleDot,
  HelpCircle,
  LogOut,
  Menu,
  MessageSquare,
  Search,
  Settings,
  User,
} from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/ThemeToggle";
import { cn } from "@/lib/utils";

interface TopBarProps {
  userName: string;
  userEmail: string;
  roles: string[];
  onLogout: () => void;
  onMobileMenu: () => void;
}

const notifications = [
  { id: "n1", title: "5 lecturers awaiting approval", time: "2m ago", unread: true },
  { id: "n2", title: "Payment gateway reconciled", time: "40m ago", unread: true },
  { id: "n3", title: "Backup completed successfully", time: "1h ago", unread: false },
  { id: "n4", title: "SignalR hub latency elevated", time: "2h ago", unread: false },
];

export function TopBar({ userName, userEmail, roles, onLogout, onMobileMenu }: TopBarProps) {
  const [openMenu, setOpenMenu] = useState<"notifications" | "profile" | null>(null);
  const unread = notifications.filter((n) => n.unread).length;

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-3 border-b border-border bg-card/80 px-4 backdrop-blur-md">
      <Button variant="ghost" size="icon" className="lg:hidden" onClick={onMobileMenu} aria-label="Menu">
        <Menu className="h-5 w-5" />
      </Button>

      {/* Global search */}
      <div className="relative hidden max-w-md flex-1 sm:block">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <input
          type="search"
          placeholder="Search users, courses, reports…"
          className="h-9 w-full rounded-xl border border-border bg-background pl-9 pr-16 text-sm outline-none transition-colors focus:border-primary/40 focus:ring-2 focus:ring-primary/20"
        />
        <kbd className="absolute right-2.5 top-1/2 hidden -translate-y-1/2 rounded border border-border bg-muted px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground md:block">
          ⌘K
        </kbd>
      </div>

      <div className="ml-auto flex items-center gap-1">
        {/* Semester chip */}
        <span className="hidden items-center gap-1.5 rounded-full border border-border px-3 py-1 text-xs font-medium xl:inline-flex">
          <CircleDot className="h-3 w-3 text-primary" />
          Fall 2026
        </span>
        {/* System status */}
        <span className="hidden items-center gap-1.5 rounded-full bg-success/10 px-3 py-1 text-xs font-medium text-success md:inline-flex">
          <span className="h-1.5 w-1.5 rounded-full bg-success" />
          Operational
        </span>

        <Button variant="ghost" size="icon" aria-label="Help center" className="hidden sm:inline-flex">
          <HelpCircle className="h-5 w-5" />
        </Button>
        <Button variant="ghost" size="icon" aria-label="Messages" className="relative hidden sm:inline-flex">
          <MessageSquare className="h-5 w-5" />
          <span className="absolute right-1.5 top-1.5 h-2 w-2 rounded-full bg-primary" />
        </Button>

        {/* Notifications */}
        <div className="relative">
          <Button
            variant="ghost"
            size="icon"
            aria-label="Notifications"
            className="relative"
            onClick={() => setOpenMenu((m) => (m === "notifications" ? null : "notifications"))}
          >
            <Bell className="h-5 w-5" />
            {unread > 0 && (
              <span className="absolute right-1 top-1 flex h-4 w-4 items-center justify-center rounded-full bg-destructive text-[10px] font-semibold text-white">
                {unread}
              </span>
            )}
          </Button>
          {openMenu === "notifications" && (
            <Dropdown onClose={() => setOpenMenu(null)}>
              <div className="flex items-center justify-between border-b border-border px-4 py-2.5">
                <p className="text-sm font-semibold">Notifications</p>
                <button className="text-xs font-medium text-primary hover:underline">Mark all read</button>
              </div>
              <ul className="max-h-80 overflow-y-auto">
                {notifications.map((n) => (
                  <li key={n.id} className="flex gap-2.5 px-4 py-2.5 hover:bg-muted">
                    <span className={cn("mt-1.5 h-2 w-2 shrink-0 rounded-full", n.unread ? "bg-primary" : "bg-transparent")} />
                    <div className="min-w-0">
                      <p className="text-sm leading-snug">{n.title}</p>
                      <p className="text-xs text-muted-foreground">{n.time}</p>
                    </div>
                  </li>
                ))}
              </ul>
            </Dropdown>
          )}
        </div>

        <ThemeToggle />

        {/* Profile */}
        <div className="relative">
          <button
            onClick={() => setOpenMenu((m) => (m === "profile" ? null : "profile"))}
            className="ml-1 flex items-center gap-2 rounded-full p-0.5 pr-2 transition-colors hover:bg-muted"
          >
            <Avatar name={userName} size="sm" status="online" />
            <span className="hidden text-left md:block">
              <span className="block text-xs font-semibold leading-tight">{userName}</span>
              <span className="block text-[11px] leading-tight text-muted-foreground">{roles[0]}</span>
            </span>
          </button>
          {openMenu === "profile" && (
            <Dropdown onClose={() => setOpenMenu(null)}>
              <div className="border-b border-border px-4 py-3">
                <p className="text-sm font-semibold">{userName}</p>
                <p className="truncate text-xs text-muted-foreground">{userEmail}</p>
              </div>
              <ul className="p-1">
                <MenuItem icon={User} label="Profile" />
                <MenuItem icon={Settings} label="Account settings" />
                <MenuItem icon={HelpCircle} label="Help & support" />
              </ul>
              <div className="border-t border-border p-1">
                <button
                  onClick={onLogout}
                  className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-destructive hover:bg-destructive/10"
                >
                  <LogOut className="h-4 w-4" />
                  Sign out
                </button>
              </div>
            </Dropdown>
          )}
        </div>
      </div>
    </header>
  );
}

function Dropdown({ children, onClose }: { children: React.ReactNode; onClose: () => void }) {
  return (
    <>
      <div className="fixed inset-0 z-40" onClick={onClose} aria-hidden />
      <div className="absolute right-0 z-50 mt-2 w-72 origin-top-right animate-fade-in overflow-hidden rounded-xl border border-border bg-card shadow-lg">
        {children}
      </div>
    </>
  );
}

function MenuItem({ icon: Icon, label }: { icon: typeof User; label: string }) {
  return (
    <li>
      <button className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm hover:bg-muted">
        <Icon className="h-4 w-4 text-muted-foreground" />
        {label}
      </button>
    </li>
  );
}
