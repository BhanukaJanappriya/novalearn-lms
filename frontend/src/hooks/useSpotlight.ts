import { useRef, type MouseEvent } from "react";

/**
 * Cursor-reactive spotlight. Tracks the pointer within an element and writes its
 * position to CSS custom properties (`--spot-x` / `--spot-y`), which the `.spotlight`
 * class turns into a soft radial glow that follows the cursor. Uses direct style
 * mutation (no React re-render) so it stays smooth.
 */
export function useSpotlight<T extends HTMLElement>() {
  const ref = useRef<T>(null);

  const onMouseMove = (event: MouseEvent<T>) => {
    const element = ref.current;
    if (!element) return;
    const rect = element.getBoundingClientRect();
    element.style.setProperty("--spot-x", `${event.clientX - rect.left}px`);
    element.style.setProperty("--spot-y", `${event.clientY - rect.top}px`);
  };

  return { ref, onMouseMove };
}
