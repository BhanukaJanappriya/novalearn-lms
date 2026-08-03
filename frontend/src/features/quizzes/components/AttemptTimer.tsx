import { useEffect, useState } from "react";
import { Clock } from "lucide-react";
import { cn } from "@/lib/utils";
import { formatRemaining } from "../lib/quizzes";

interface AttemptTimerProps {
  deadlineUtc: string;
  /** Called once when the clock reaches zero. */
  onExpire: () => void;
}

/**
 * Counts down to the deadline and fires once at zero.
 *
 * The clock is a courtesy, not the rule: the server records how long the attempt actually took
 * and flags a late submission regardless of what the browser did.
 */
export function AttemptTimer({ deadlineUtc, onExpire }: AttemptTimerProps) {
  const [remaining, setRemaining] = useState(() => new Date(deadlineUtc).getTime() - Date.now());

  useEffect(() => {
    const deadline = new Date(deadlineUtc).getTime();
    let fired = false;

    const tick = () => {
      const left = deadline - Date.now();
      setRemaining(left);

      if (left <= 0 && !fired) {
        fired = true;
        onExpire();
      }
    };

    tick();
    const id = window.setInterval(tick, 1000);
    return () => window.clearInterval(id);
  }, [deadlineUtc, onExpire]);

  const urgent = remaining <= 60_000;

  return (
    <span
      role="timer"
      aria-live={urgent ? "assertive" : "off"}
      className={cn(
        "inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1 text-sm font-semibold tabular-nums",
        urgent ? "bg-destructive/10 text-destructive" : "bg-muted text-muted-foreground",
      )}
    >
      <Clock className="h-4 w-4" aria-hidden />
      {formatRemaining(remaining)}
    </span>
  );
}
