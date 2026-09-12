import { useEffect, useRef, useState } from "react";
import { cn } from "@/lib/utils";

interface ScrollRevealProps {
  children: React.ReactNode;
  className?: string;
  /** Stagger delay in ms, applied as a transition-delay once the section is in view. */
  delay?: number;
}

/**
 * Fades and rises a section into place the first time it scrolls into view, via
 * IntersectionObserver plus a plain CSS transition — not framer-motion. A stack of framer mount
 * animations on this same brand panel proved unreliable in this app (see AuthShowcase's history);
 * a CSS transition toggled by a class never has that problem.
 *
 * Shows content immediately, un-observed, under reduced motion. Disconnects after the first
 * reveal — sections stay revealed once seen rather than re-animating on every scroll past.
 */
export function ScrollReveal({ children, className, delay = 0 }: ScrollRevealProps) {
  const ref = useRef<HTMLDivElement>(null);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      setVisible(true);
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setVisible(true);
          observer.disconnect();
        }
      },
      { threshold: 0.2, rootMargin: "0px 0px -10% 0px" },
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return (
    <div
      ref={ref}
      className={cn(
        "transition-all duration-700 ease-[cubic-bezier(0.22,1,0.36,1)]",
        visible ? "translate-y-0 opacity-100" : "translate-y-6 opacity-0",
        className,
      )}
      style={{ transitionDelay: visible ? `${delay}ms` : "0ms" }}
    >
      {children}
    </div>
  );
}
