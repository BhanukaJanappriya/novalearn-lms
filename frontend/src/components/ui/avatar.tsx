import { useEffect, useState } from "react";
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
  xl: "h-24 w-24 text-2xl",
} as const;

export interface AvatarProps {
  name: string;
  /** The person's picture. Falls back to their initials if absent or unreachable. */
  src?: string | null;
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

/**
 * Circular avatar with an optional presence dot.
 *
 * Initials render underneath at all times and the picture fades in over them, so a slow or
 * broken image leaves a readable avatar rather than a blank circle. Every consumer of this
 * component therefore works whether or not anyone has set a picture.
 */
export function Avatar({ name, src, color, size = "md", status, className }: AvatarProps) {
  const [loaded, setLoaded] = useState(false);
  const [failed, setFailed] = useState(false);

  // A changed picture must not keep the previous one's loaded state.
  useEffect(() => {
    setLoaded(false);
    setFailed(false);
  }, [src]);

  const showImage = Boolean(src) && !failed;

  return (
    <span className={cn("relative inline-flex shrink-0", className)}>
      <span
        className={cn(
          "relative inline-flex items-center justify-center overflow-hidden rounded-full font-semibold text-white ring-2 ring-card",
          sizeClass[size],
        )}
        style={{ backgroundColor: color ?? "hsl(258 90% 66%)" }}
      >
        <span aria-hidden>{initials(name)}</span>

        {showImage && (
          <img
            src={src!}
            alt=""
            aria-hidden
            loading="lazy"
            decoding="async"
            onLoad={() => setLoaded(true)}
            onError={() => setFailed(true)}
            className={cn(
              "absolute inset-0 h-full w-full object-cover transition-opacity duration-500 ease-out",
              loaded ? "opacity-100" : "opacity-0",
            )}
          />
        )}
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
