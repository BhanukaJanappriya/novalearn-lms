import { cn } from "@/lib/utils";

type OnlineStatus = "online" | "away" | "offline";

const statusColor: Record<OnlineStatus, string> = {
  online: "bg-success",
  away: "bg-[hsl(var(--warning))]",
  offline: "bg-muted-foreground/40",
};

const sizeClass = {
  sm: "h-8 w-8 text-xs",
  md: "h-10 w-10 text-sm",
  lg: "h-12 w-12 text-base",
} as const;

export interface AvatarProps {
  name: string;
  /** Any CSS color (used as a tinted background). Falls back to brand purple. */
  color?: string;
  size?: keyof typeof sizeClass;
  status?: OnlineStatus;
  className?: string;
}

function initials(name: string): string {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

/** Circular initials avatar with an optional presence dot. */
export function Avatar({ name, color, size = "md", status, className }: AvatarProps) {
  return (
    <span className={cn("relative inline-flex shrink-0", className)}>
      <span
        className={cn(
          "inline-flex items-center justify-center rounded-full font-semibold text-white ring-2 ring-card",
          sizeClass[size],
        )}
        style={{ backgroundColor: color ?? "hsl(258 90% 66%)" }}
        aria-hidden
      >
        {initials(name)}
      </span>
      {status && (
        <span
          className={cn(
            "absolute bottom-0 right-0 h-2.5 w-2.5 rounded-full ring-2 ring-card",
            statusColor[status],
          )}
          aria-label={status}
        />
      )}
    </span>
  );
}
