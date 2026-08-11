/**
 * Preset profile pictures.
 *
 * There is no file storage in this project yet, so a picture is a URL rather than an upload.
 * DiceBear generates a stable illustrated avatar from a seed, is free, needs no key, and gives
 * people something reasonable to choose without hosting an image anywhere. Anyone who does have
 * a hosted picture can paste its address instead.
 */
const DICEBEAR = "https://api.dicebear.com/9.x";

/** Styles offered in the picker, in the order they appear. */
export const AVATAR_STYLES = [
  { id: "notionists", label: "Illustrated" },
  { id: "avataaars", label: "Character" },
  { id: "thumbs", label: "Simple" },
  { id: "bottts", label: "Robot" },
] as const;

export type AvatarStyle = (typeof AVATAR_STYLES)[number]["id"];

/** A preset picture for one style and seed. The same pair always gives the same picture. */
export function presetAvatarUrl(style: AvatarStyle, seed: string): string {
  return `${DICEBEAR}/${style}/svg?seed=${encodeURIComponent(seed)}`;
}

/**
 * A handful of variations per style, seeded from the person so their options are their own
 * rather than the same four faces everyone else sees.
 */
export function presetsFor(style: AvatarStyle, seed: string, count = 8): string[] {
  return Array.from({ length: count }, (_, index) => presetAvatarUrl(style, `${seed}-${index}`));
}

/** Whether a picture link is one the API will accept, mirrored so the form can say so first. */
export function isUsablePictureUrl(value: string): boolean {
  if (value.trim().length === 0) {
    return true;
  }

  try {
    const url = new URL(value.trim());
    return url.protocol === "http:" || url.protocol === "https:";
  } catch {
    return false;
  }
}
