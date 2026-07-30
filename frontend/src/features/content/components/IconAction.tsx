import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

/** Compact icon-only action used by the module and lesson rows. */
export function IconAction({
  icon: Icon,
  label,
  onClick,
  disabled,
  tone = "default",
}: {
  icon: LucideIcon;
  /** Accessible name; also the tooltip. */
  label: string;
  onClick: () => void;
  disabled?: boolean;
  tone?: "default" | "destructive";
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      onClick={onClick}
      disabled={disabled}
      className={cn(
        "rounded-lg p-1.5 transition-colors disabled:pointer-events-none disabled:opacity-30",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        tone === "destructive"
          ? "text-destructive hover:bg-destructive/10"
          : "text-muted-foreground hover:bg-muted hover:text-foreground",
      )}
    >
      <Icon className="h-4 w-4" aria-hidden />
    </button>
  );
}
