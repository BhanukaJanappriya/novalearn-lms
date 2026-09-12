import { useEffect, useRef } from "react";
import { cn } from "@/lib/utils";

interface Blob {
  className: string;
  size: number;
  top: string;
  left: string;
  duration: number;
  delay: number;
  /** Parallax reach, 0..1 — how far this blob drifts toward the pointer at full range. */
  depth: number;
}

const BLOBS: Blob[] = [
  { className: "bg-brand-foreground/20", size: 320, top: "-10%", left: "-12%", duration: 17, delay: 0, depth: 0.5 },
  { className: "bg-accent/35", size: 260, top: "48%", left: "68%", duration: 21, delay: 2.5, depth: 1 },
  { className: "bg-brand-foreground/10", size: 220, top: "76%", left: "-8%", duration: 19, delay: 5, depth: 0.7 },
];

/**
 * Soft, continuously morphing background shapes for the brand panel. Each blob runs the same
 * `blob-morph` keyframe (index.css) at its own duration/delay so they never sync into one obvious
 * repeat — that's the "different morph effects".
 *
 * Cursor parallax and the morph are deliberately split across two nested elements per blob: an
 * inline `style.transform` written every frame by this component always wins over a CSS class's
 * `transform` for the *same* element (the exact gotcha CursorField's own comments warn about), so
 * mixing pointer-follow and the CSS keyframe morph on one node would silently drop one of them.
 * The outer node only ever receives the JS transform; the inner one only ever receives the CSS
 * keyframe's.
 */
export function MorphingBlobs() {
  const panelRef = useRef<HTMLDivElement>(null);
  const wrapRefs = useRef<(HTMLDivElement | null)[]>([]);

  useEffect(() => {
    const panel = panelRef.current?.parentElement;
    if (!panel) return;
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

    let frame = 0;
    let targetX = 0;
    let targetY = 0;
    const current = BLOBS.map(() => ({ x: 0, y: 0 }));

    const onMove = (event: MouseEvent) => {
      const rect = panel.getBoundingClientRect();
      // -1..1 from the panel's centre, so every blob can scale the same reading by its own depth.
      targetX = ((event.clientX - rect.left) / rect.width) * 2 - 1;
      targetY = ((event.clientY - rect.top) / rect.height) * 2 - 1;
    };

    const tick = () => {
      BLOBS.forEach((blob, i) => {
        const state = current[i]!;
        const reach = 20 * blob.depth;
        state.x += (targetX * reach - state.x) * 0.06;
        state.y += (targetY * reach - state.y) * 0.06;
        const el = wrapRefs.current[i];
        if (el) el.style.transform = `translate3d(${state.x.toFixed(1)}px, ${state.y.toFixed(1)}px, 0)`;
      });
      frame = requestAnimationFrame(tick);
    };

    document.addEventListener("mousemove", onMove, { passive: true });
    frame = requestAnimationFrame(tick);
    return () => {
      document.removeEventListener("mousemove", onMove);
      cancelAnimationFrame(frame);
    };
  }, []);

  return (
    <div ref={panelRef} className="pointer-events-none absolute inset-0 overflow-hidden" aria-hidden>
      {BLOBS.map((blob, i) => (
        <div
          key={i}
          ref={(el) => {
            wrapRefs.current[i] = el;
          }}
          className="absolute"
          style={{ top: blob.top, left: blob.left, width: blob.size, height: blob.size }}
        >
          <div
            className={cn("blob-morph h-full w-full blur-2xl", blob.className)}
            style={{ animationDuration: `${blob.duration}s`, animationDelay: `${blob.delay}s` }}
          />
        </div>
      ))}
    </div>
  );
}
