import { useEffect, useRef, useState } from "react";

/**
 * Animates a number from 0 → `value` once it first becomes non-null, using an
 * ease-out curve. Respects `prefers-reduced-motion` (snaps instantly).
 */
export function useCountUp(value: number, durationMs = 1100): number {
  const [display, setDisplay] = useState(0);
  const frame = useRef<number>(0);
  const startValue = useRef(0);

  useEffect(() => {
    const reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reduced) {
      setDisplay(value);
      return;
    }

    const from = startValue.current;
    const start = performance.now();

    const tick = (now: number) => {
      const t = Math.min(1, (now - start) / durationMs);
      const eased = 1 - Math.pow(1 - t, 3); // ease-out cubic
      const current = from + (value - from) * eased;
      setDisplay(current);
      if (t < 1) {
        frame.current = requestAnimationFrame(tick);
      } else {
        startValue.current = value;
      }
    };

    frame.current = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(frame.current);
  }, [value, durationMs]);

  return display;
}
