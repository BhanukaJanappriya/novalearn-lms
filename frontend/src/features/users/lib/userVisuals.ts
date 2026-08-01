type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

/**
 * Role colours ordered by authority, so a glance at the table conveys privilege level.
 * Unknown roles fall back to neutral rather than throwing.
 */
const roleVariants: Record<string, BadgeVariant> = {
  SuperAdministrator: "destructive",
  Administrator: "warning",
  Lecturer: "default",
  TeachingAssistant: "success",
  Student: "neutral",
  Guest: "outline",
};

export function roleVariant(role: string): BadgeVariant {
  return roleVariants[role] ?? "neutral";
}

/** Rank used to sort a user's roles so the most privileged reads first. */
const roleRank: Record<string, number> = {
  SuperAdministrator: 0,
  Administrator: 1,
  Lecturer: 2,
  TeachingAssistant: 3,
  Student: 4,
  Guest: 5,
};

export function sortRoles(roles: string[]): string[] {
  return [...roles].sort((a, b) => (roleRank[a] ?? 99) - (roleRank[b] ?? 99));
}

/** Splits a display name into up to two initials for the avatar. */
export function initials(fullName: string): string {
  return fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

const palette = ["#8B5CF6", "#2a78d6", "#1baf7a", "#eda100", "#4a3aa7", "#e34948"];

/** Deterministic avatar colour, matching the roster treatment elsewhere. */
export function avatarColor(seed: string): string {
  const hash = [...seed].reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
  return palette[hash % palette.length];
}
