import { useCallback, useEffect, useState } from "react";
import { Quote } from "lucide-react";
import { cn } from "@/lib/utils";
import { testimonials } from "./testimonialData";
import { StarRating } from "./StarRating";

/**
 * Rotating customer quotes for the auth brand panel.
 *
 * All slides live in one flex track that translates by whole panels, so the active quote is
 * always mounted and the "tab moving" transition is a single CSS transform transition — no
 * animation library, nothing to get stuck. The tab indicator slides the same way. The global
 * reduced-motion rule in index.css collapses both transitions.
 *
 * Auto-advances every 6s; the timer is skipped under reduced motion and pauses on pointer hover.
 */

const ROTATE_MS = 6000;
/** Tab pill width (w-8 = 2rem) plus the gap (gap-2 = 0.5rem), in px — the indicator's stride. */
const TAB_STRIDE = 40;

/** Drop honorifics ("Dr.", "Prof.") so initials read from the actual name. */
const HONORIFIC = /^(dr|prof|mr|mrs|ms|mx)\.?$/i;

function initials(name: string): string {
  return name
    .split(/\s+/)
    .filter((part) => part && !HONORIFIC.test(part))
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("");
}

export function TestimonialCarousel() {
  const count = testimonials.length;
  const [index, setIndex] = useState(0);
  const [paused, setPaused] = useState(false);

  const go = useCallback(
    (next: number) => setIndex(((next % count) + count) % count),
    [count],
  );

  // One-shot timer, re-armed on every `index` change — so any interaction that moves the index
  // also resets the countdown. Skipped while paused or when the user prefers reduced motion.
  useEffect(() => {
    if (paused) return;
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    const id = window.setTimeout(() => setIndex((i) => (i + 1) % count), ROTATE_MS);
    return () => window.clearTimeout(id);
  }, [index, paused, count]);

  const onKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
    if (event.key === "ArrowRight") {
      event.preventDefault();
      go(index + 1);
    } else if (event.key === "ArrowLeft") {
      event.preventDefault();
      go(index - 1);
    }
  };

  return (
    <div
      className="relative w-full"
      onMouseEnter={() => setPaused(true)}
      onMouseLeave={() => setPaused(false)}
    >
      <div className="overflow-hidden rounded-2xl border border-brand-foreground/15 bg-brand-foreground/10 backdrop-blur-sm">
        {/*
          One track `count` panels wide (`flex-1` gives each panel an equal 1/count share, so a
          panel is exactly the card's width). Shifting by `100/count %` of the track = one panel.
        */}
        <div
          className="flex transition-transform duration-500 ease-[cubic-bezier(0.22,1,0.36,1)]"
          style={{
            width: `${count * 100}%`,
            transform: `translateX(-${index * (100 / count)}%)`,
          }}
        >
          {testimonials.map((t, i) => (
            <div key={t.name} className="min-w-0 flex-1 p-5" aria-hidden={i !== index}>
              <Quote className="h-6 w-6 text-brand-foreground/25" aria-hidden />

              {/* Fixed min-height, quote centred in it, so the card never resizes between slides. */}
              <div className="mt-2 flex min-h-[7rem] items-center">
                <p className="font-medium leading-relaxed text-brand-foreground">{t.quote}</p>
              </div>

              <div className="mt-3.5">
                <StarRating value={t.rating} size={15} className="text-amber-300" />
                <div className="mt-3 flex items-center gap-3">
                  <span
                    aria-hidden
                    className="grid h-9 w-9 place-items-center rounded-full text-xs font-semibold text-white"
                    style={{ backgroundColor: t.accent ?? "hsl(258 90% 66%)" }}
                  >
                    {initials(t.name)}
                  </span>
                  <span className="flex flex-col">
                    <span className="text-sm font-semibold text-brand-foreground">{t.name}</span>
                    <span className="text-xs text-brand-foreground/70">
                      {t.role + " · " + t.institution}
                    </span>
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div
        role="tablist"
        aria-label="Testimonials"
        onKeyDown={onKeyDown}
        className="relative mt-3.5 flex gap-2"
      >
        <span
          aria-hidden
          className="absolute left-0 top-0 h-1.5 w-8 rounded-full bg-brand-foreground transition-transform duration-300 ease-out"
          style={{ transform: `translateX(${index * TAB_STRIDE}px)` }}
        />
        {testimonials.map((t, i) => (
          <button
            key={t.name}
            type="button"
            role="tab"
            aria-selected={i === index}
            aria-label={`Show testimonial ${i + 1}`}
            onClick={() => go(i)}
            className={cn(
              "h-1.5 w-8 rounded-full outline-none transition-colors focus-visible:ring-2 focus-visible:ring-brand-foreground/50",
              i === index ? "bg-transparent" : "bg-brand-foreground/20 hover:bg-brand-foreground/35",
            )}
          />
        ))}
      </div>
    </div>
  );
}
