import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  BarChart3,
  BookPlus,
  DatabaseBackup,
  Megaphone,
  PartyPopper,
  ShieldCheck,
  UserCheck,
  UserPlus,
  type LucideIcon,
} from "lucide-react";
import { Confetti } from "./Confetti";

interface QuickAction {
  label: string;
  icon: LucideIcon;
  /** Admin route this action opens. */
  to: string;
  /** Marks the celebratory "big" action that fires confetti before navigating. */
  celebrate?: boolean;
}

const actions: QuickAction[] = [
  { label: "Approve Users", icon: UserCheck, to: "/admin/users" },
  { label: "Create Course", icon: BookPlus, to: "/admin/courses" },
  { label: "Add Lecturer", icon: UserPlus, to: "/admin/lecturers" },
  { label: "Send Announcement", icon: Megaphone, to: "/admin/content" },
  { label: "Generate Report", icon: BarChart3, to: "/admin/reports" },
  { label: "Backup Database", icon: DatabaseBackup, to: "/admin/settings" },
  { label: "Security Center", icon: ShieldCheck, to: "/admin/security" },
  { label: "Publish Semester", icon: PartyPopper, to: "/admin/courses", celebrate: true },
];

const cardClass =
  "group flex flex-col items-center gap-2 rounded-xl border border-border bg-background/40 p-3 text-center transition-all hover:-translate-y-0.5 hover:border-primary/30 hover:shadow-soft focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background";

export function QuickActions() {
  const [celebrating, setCelebrating] = useState(false);
  const navigate = useNavigate();

  return (
    <div className="relative grid grid-cols-2 gap-2 sm:grid-cols-4">
      {celebrating && <Confetti />}
      {actions.map((action) => {
        const Icon = action.icon;
        const content = (
          <>
            <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary transition-colors group-hover:bg-primary group-hover:text-primary-foreground">
              <Icon className="h-4 w-4" aria-hidden />
            </span>
            <span className="text-xs font-medium leading-tight">{action.label}</span>
          </>
        );

        if (action.celebrate) {
          return (
            <button
              key={action.label}
              type="button"
              onClick={() => {
                setCelebrating(true);
                // Let the burst play, then head to the destination.
                window.setTimeout(() => {
                  setCelebrating(false);
                  navigate(action.to);
                }, 1400);
              }}
              className={cardClass}
            >
              {content}
            </button>
          );
        }

        return (
          <Link key={action.label} to={action.to} className={cardClass}>
            {content}
          </Link>
        );
      })}
    </div>
  );
}
