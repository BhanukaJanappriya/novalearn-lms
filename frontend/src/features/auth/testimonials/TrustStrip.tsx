/**
 * TrustStrip — social-proof block for the AuthLayout brand panel.
 *
 * Sits on brand purple, so every colour is a `text-primary-foreground` shade (the lone literal
 * being `text-amber-300` for the stars). The figures show at their final value; the tiles fade in
 * with a small stagger via the tailwind `animate-fade-in` keyframe.
 */
import { formatCount } from "./format";
import { StarRating } from "./StarRating";

interface Stat {
  value: number;
  /** Appended after the formatted number, so 12,000 reads as "12,000+". */
  suffix: string;
  label: string;
}

const STATS: Stat[] = [
  { value: 12000, suffix: "+", label: "Active learners" },
  { value: 40, suffix: "+", label: "Institutions" },
  { value: 96, suffix: "%", label: "Course completion" },
];

export function TrustStrip() {
  return (
    <div className="relative flex flex-col gap-3.5">
      {/* Aggregate rating */}
      <div className="flex items-center gap-2">
        <StarRating value={4.9} size={16} className="text-amber-300" />
        <span className="font-semibold text-primary-foreground">4.9</span>
        <span className="text-xs text-primary-foreground/70">from 1,200+ reviews</span>
      </div>

      {/* Headline figures */}
      <div className="grid grid-cols-3 gap-4 border-t border-primary-foreground/15 pt-3.5">
        {STATS.map((stat, i) => (
          <div
            key={stat.label}
            className="flex flex-col gap-0.5 animate-fade-in [animation-fill-mode:backwards]"
            style={{ animationDelay: `${0.25 + i * 0.08}s` }}
          >
            <span className="text-2xl font-semibold tabular-nums text-primary-foreground">
              {formatCount(stat.value)}
              {stat.suffix}
            </span>
            <span className="text-xs text-primary-foreground/70">{stat.label}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
