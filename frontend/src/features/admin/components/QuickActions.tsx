import { useState } from "react";
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
  /** Marks the celebratory "big" action that fires confetti. */
  celebrate?: boolean;
}

const actions: QuickAction[] = [
  { label: "Approve Users", icon: UserCheck },
  { label: "Create Course", icon: BookPlus },
  { label: "Add Lecturer", icon: UserPlus },
  { label: "Send Announcement", icon: Megaphone },
  { label: "Generate Report", icon: BarChart3 },
  { label: "Backup Database", icon: DatabaseBackup },
  { label: "Security Center", icon: ShieldCheck },
  { label: "Publish Semester", icon: PartyPopper, celebrate: true },
];

export function QuickActions() {
  const [celebrating, setCelebrating] = useState(false);

  return (
    <div className="relative grid grid-cols-2 gap-2 sm:grid-cols-4">
      {celebrating && <Confetti />}
      {actions.map((action) => {
        const Icon = action.icon;
        return (
          <button
            key={action.label}
            type="button"
            onClick={() => {
              if (action.celebrate) {
                setCelebrating(true);
                window.setTimeout(() => setCelebrating(false), 1400);
              }
            }}
            className="group flex flex-col items-center gap-2 rounded-xl border border-border bg-background/40 p-3 text-center transition-all hover:-translate-y-0.5 hover:border-primary/30 hover:shadow-soft"
          >
            <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary transition-colors group-hover:bg-primary group-hover:text-primary-foreground">
              <Icon className="h-4 w-4" />
            </span>
            <span className="text-xs font-medium leading-tight">{action.label}</span>
          </button>
        );
      })}
    </div>
  );
}
