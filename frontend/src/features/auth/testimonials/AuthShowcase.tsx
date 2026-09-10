import { TestimonialCarousel } from "./TestimonialCarousel";
import { TrustStrip } from "./TrustStrip";

/**
 * Social proof for the sign-in brand panel: a rotating customer quote with its rating, over a
 * strip of aggregate trust figures. Purely presentational and self-contained; it only renders
 * where the brand panel does (large screens). Width is set by the parent column.
 *
 * Entrance and transitions here are plain CSS (the tailwind `animate-fade-in` keyframe, transform
 * transitions in the carousel) rather than a motion library: it is the right tool for the job and
 * it inherits the global reduced-motion rule in index.css for free.
 */
export function AuthShowcase() {
  return (
    <div
      className="relative space-y-4 animate-fade-in [animation-fill-mode:backwards]"
      style={{ animationDelay: "0.15s" }}
    >
      <TestimonialCarousel />
      <TrustStrip />
    </div>
  );
}
