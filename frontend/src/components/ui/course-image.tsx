import { useState } from "react";
import { cn } from "@/lib/utils";
import { courseImageUrl } from "@/lib/imagery";

interface CourseImageProps {
  code: string;
  category?: string | null;
  /** An explicit cover set by the lecturer always wins over the subject photo. */
  coverImageUrl?: string | null;
  /** The gradient shown underneath, and left visible if the photo never arrives. */
  gradient: string;
  width?: number;
  height?: number;
  className?: string;
  children?: React.ReactNode;
}

/**
 * A course cover photo layered over its brand gradient.
 *
 * The gradient renders immediately and the photo fades in on top once decoded, so the card never
 * flashes an empty box and never shifts layout. If the CDN is unreachable, or the user is
 * offline, the gradient simply stays: the design already worked without photographs, so a failed
 * image degrades rather than breaks.
 */
export function CourseImage({
  code,
  category,
  coverImageUrl,
  gradient,
  width = 800,
  height = 400,
  className,
  children,
}: CourseImageProps) {
  const [loaded, setLoaded] = useState(false);
  const [failed, setFailed] = useState(false);

  const src = courseImageUrl(category, code, coverImageUrl, { width, height });

  return (
    <div
      className={cn("relative overflow-hidden", className)}
      style={{ backgroundImage: gradient }}
    >
      {!failed && (
        <img
          src={src}
          alt=""
          aria-hidden
          loading="lazy"
          decoding="async"
          onLoad={() => setLoaded(true)}
          onError={() => setFailed(true)}
          className={cn(
            "absolute inset-0 h-full w-full object-cover transition-opacity duration-700 ease-out",
            loaded ? "opacity-100" : "opacity-0",
          )}
        />
      )}

      {/* Keeps overlaid badges and titles legible whatever the photo underneath looks like. */}
      <div className="absolute inset-0 bg-gradient-to-t from-black/55 via-black/20 to-transparent" />

      {children}
    </div>
  );
}
