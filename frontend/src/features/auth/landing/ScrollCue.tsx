import { useEffect, useState, type RefObject } from "react";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface ScrollCueProps {
  targetRef: RefObject<HTMLDivElement | null>;
}

/**
 * A small bouncing "scroll for more" hint over the brand panel's hero, shown until the visitor
 * scrolls the panel themselves — the panel's own scrollbar is hidden for a cleaner look, so this
 * is what tells anyone there is more below the fold.
 */
export function ScrollCue({ targetRef }: ScrollCueProps) {
  const [dismissed, setDismissed] = useState(false);

  useEffect(() => {
    const el = targetRef.current;
    if (!el) return;

    const onScroll = () => {
      if (el.scrollTop > 24) setDismissed(true);
    };
    el.addEventListener("scroll", onScroll, { passive: true });
    return () => el.removeEventListener("scroll", onScroll);
  }, [targetRef]);

  return (
    <div
      aria-hidden
      className={cn(
        "pointer-events-none absolute bottom-6 left-1/2 flex -translate-x-1/2 flex-col items-center gap-1 text-brand-foreground/60 transition-opacity duration-500",
        dismissed ? "opacity-0" : "opacity-100",
      )}
    >
      <span className="text-[0.65rem] font-medium uppercase tracking-widest">Scroll to explore</span>
      <ChevronDown className="scroll-cue-bob h-4 w-4" />
    </div>
  );
}
