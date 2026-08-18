import { useEffect, useRef } from "react";

/**
 * A custom cursor: a tight dot plus a ring that trails a step behind it. The ring grows and
 * brightens over anything clickable, and shrinks on the click itself. It replaces the old
 * per-card spotlight glow with one global effect that reads as "this whole product responds to
 * you" rather than "this one card does".
 *
 * Position is driven by direct style mutation inside a single rAF loop, the same technique the
 * spotlight it replaces used, rather than React state: a mousemove-driven re-render would fight
 * the browser's own paint budget on a page with many cards underneath it.
 *
 * Stays off entirely under two conditions, checked once on mount rather than watched live (an OS
 * setting changing mid-session is rare enough that a reload to pick it up is a fair trade for one
 * fewer live listener):
 *   - no fine pointer (touch/stylus primary input) — there is no cursor to replace;
 *   - `prefers-reduced-motion` — a shape trailing the finger is exactly the motion that asks for.
 * The native cursor is hidden by a plain CSS media query on the same two conditions rather than a
 * JS-toggled class, so there is no possible frame where neither cursor is visible while this
 * component's effect is still warming up.
 */
export function CursorField() {
  const dotRef = useRef<HTMLDivElement>(null);
  const ringRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const canHover =
      window.matchMedia("(pointer: fine)").matches &&
      !window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (!canHover) return;

    const dot = dotRef.current;
    const ring = ringRef.current;
    if (!dot || !ring) return;

    // The dot tracks almost exactly; the ring eases toward the same point a beat later, which is
    // what reads as "trailing" rather than as two copies of the same dot.
    const pointer = { x: window.innerWidth / 2, y: window.innerHeight / 2 };
    const ringPos = { ...pointer };
    let ringScale = 1;
    let visible = false;
    let frame = 0;

    const show = () => {
      if (visible) return;
      visible = true;
      dot.style.opacity = "1";
      ring.style.opacity = "1";
    };

    const hide = () => {
      visible = false;
      dot.style.opacity = "0";
      ring.style.opacity = "0";
    };

    const onMove = (event: MouseEvent) => {
      pointer.x = event.clientX;
      pointer.y = event.clientY;
      show();
    };

    // A cross-origin iframe (a course's YouTube embed, a PDF viewer) owns its own cursor once the
    // pointer is over it, and the parent document stops receiving move events for as long as it
    // stays there. Fading out on the way across the boundary avoids a cursor left stranded mid-page.
    const onOut = (event: MouseEvent) => {
      if (event.relatedTarget === null) hide();
    };

    const interactiveSelector =
      'a[href], button:not(:disabled), [role="button"], input:not([type="hidden"]):not(:disabled), ' +
      "select:not(:disabled), textarea:not(:disabled), label, summary, [data-cursor-interactive]";

    // Scale is folded into the same transform the tick loop writes every frame, rather than left
    // to a CSS class: an inline style always wins over a stylesheet rule for the same property, so
    // a scale declared in CSS on top of a JS-driven transform would simply never be seen.
    let isOverInteractive = false;
    let isPressed = false;

    const onOver = (event: MouseEvent) => {
      const target = event.target;
      isOverInteractive = target instanceof Element && target.closest(interactiveSelector) !== null;
      ring.classList.toggle("cursor-ring--active", isOverInteractive);
    };

    const onDown = () => {
      isPressed = true;
    };

    const onUp = () => {
      isPressed = false;
    };

    const tick = () => {
      // Dot: snaps to the pointer, no easing, so it never feels like it is chasing the click.
      dot.style.transform = `translate3d(${pointer.x}px, ${pointer.y}px, 0)`;

      // Ring: eases toward the pointer's last position, one fixed fraction of the remaining
      // distance per frame — a standard critically-damped lerp, independent of frame rate at the
      // 60fps this targets.
      ringPos.x += (pointer.x - ringPos.x) * 0.18;
      ringPos.y += (pointer.y - ringPos.y) * 0.18;

      // Eased the same way as position, but faster, so growing over a button reads as a quick
      // pop rather than an instant jump.
      const targetScale = isPressed ? 0.85 : isOverInteractive ? 1.7 : 1;
      ringScale += (targetScale - ringScale) * 0.3;

      ring.style.transform = `translate3d(${ringPos.x}px, ${ringPos.y}px, 0) scale(${ringScale})`;

      frame = requestAnimationFrame(tick);
    };

    document.addEventListener("mousemove", onMove, { passive: true });
    document.addEventListener("mouseover", onOver, { passive: true });
    document.addEventListener("mouseout", onOut, { passive: true });
    document.addEventListener("mousedown", onDown);
    document.addEventListener("mouseup", onUp);
    frame = requestAnimationFrame(tick);

    return () => {
      document.removeEventListener("mousemove", onMove);
      document.removeEventListener("mouseover", onOver);
      document.removeEventListener("mouseout", onOut);
      document.removeEventListener("mousedown", onDown);
      document.removeEventListener("mouseup", onUp);
      cancelAnimationFrame(frame);
    };
  }, []);

  return (
    <div aria-hidden className="pointer-events-none fixed inset-0 z-[9999]">
      <div ref={dotRef} className="cursor-dot" />
      <div ref={ringRef} className="cursor-ring" />
    </div>
  );
}
