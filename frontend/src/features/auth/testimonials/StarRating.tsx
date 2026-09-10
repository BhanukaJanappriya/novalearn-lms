import { Star } from "lucide-react";
import { cn } from "@/lib/utils";

interface StarRatingProps {
  /** 0..5, may be fractional (4.9 renders as four-and-most-of-a-fifth). */
  value: number;
  /** Star edge length in pixels. */
  size?: number;
  className?: string;
  /** Overrides the default accessible label. */
  label?: string;
}

/**
 * Five stars with a fractional fill. The colour is inherited from the current text colour, so a
 * caller tints it with a `text-*` class on a wrapper rather than a prop.
 */
export function StarRating({ value, size = 14, className, label }: StarRatingProps) {
  const clamped = Math.max(0, Math.min(5, value));
  const pct = (clamped / 5) * 100;
  const row = size * 5;

  const stars = (filled: boolean) =>
    Array.from({ length: 5 }).map((_, i) => (
      <Star
        key={i}
        strokeWidth={1.5}
        style={{ width: size, height: size }}
        className={cn("shrink-0", filled ? "fill-current" : "opacity-30")}
      />
    ));

  return (
    <span
      role="img"
      aria-label={label ?? `Rated ${clamped.toFixed(1)} out of 5`}
      className={cn("relative inline-flex", className)}
      style={{ width: row, height: size }}
    >
      <span className="absolute inset-0 flex" aria-hidden>
        {stars(false)}
      </span>
      <span className="absolute inset-0 overflow-hidden" style={{ width: `${pct}%` }} aria-hidden>
        <span className="flex" style={{ width: row }}>
          {stars(true)}
        </span>
      </span>
    </span>
  );
}
