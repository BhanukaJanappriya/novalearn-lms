/**
 * Course imagery.
 *
 * Photographs come from the Unsplash CDN, which the Unsplash License allows to be hotlinked
 * without attribution or an API key. Every id below was checked to resolve before being added.
 *
 * Choosing by category rather than storing a URL per course means a lecturer gets a sensible
 * picture for free, while an explicit `coverImageUrl` on the course always wins. Courses in the
 * same category share a photo deliberately: a consistent visual per subject reads as design
 * rather than as random stock imagery.
 */

/** Subject-appropriate photo for each category we ship demo data for. */
const CATEGORY_PHOTOS: Record<string, string> = {
  "Computer Science": "photo-1461749280684-dccba630e2f6",
  "Artificial Intelligence": "photo-1516321318423-f06f85e504b3",
  Business: "photo-1454165804606-c3d57bc86b40",
  Science: "photo-1532094349884-543bc11b234d",
  Mathematics: "photo-1509228468518-180dd4864904",
  Engineering: "photo-1581091226825-a6a2a5aee158",
};

/**
 * Used for a category we have no photo for. Picked deterministically from the course code, so a
 * given course always shows the same image rather than shuffling between renders.
 */
const FALLBACK_PHOTOS = [
  "photo-1524178232363-1fb2b075b655",
  "photo-1481627834876-b7833e8f5570",
  "photo-1434030216411-0b793f4b4173",
  "photo-1497633762265-9d179a990aa6",
];

/** Stable hash so the same seed always lands on the same photo. */
function hash(seed: string): number {
  return [...seed].reduce((acc, char) => acc + char.charCodeAt(0), 0);
}

export interface CourseImageOptions {
  /** Rendered width in CSS pixels. The CDN resizes, so we never ship a 4000px file. */
  width?: number;
  height?: number;
}

/**
 * A cover photo for a course. Prefers whatever the lecturer set, then the subject photo, then a
 * deterministic fallback.
 */
export function courseImageUrl(
  category: string | null | undefined,
  code: string,
  explicitUrl?: string | null,
  { width = 800, height = 400 }: CourseImageOptions = {},
): string {
  if (explicitUrl && explicitUrl.trim().length > 0) {
    return explicitUrl;
  }

  const photoId =
    (category && CATEGORY_PHOTOS[category]) ??
    FALLBACK_PHOTOS[hash(code || "novalearn") % FALLBACK_PHOTOS.length];

  // auto=format lets the CDN serve webp where supported; fit=crop keeps the aspect ratio honest.
  return `https://images.unsplash.com/${photoId}?auto=format&fit=crop&w=${width}&h=${height}&q=70`;
}

/** A wide, calm photo for the sign-in and register panels. */
export const AUTH_HERO_URL =
  "https://images.unsplash.com/photo-1524178232363-1fb2b075b655?auto=format&fit=crop&w=1200&h=1600&q=75";
