import {
  FileArchive,
  FileText,
  Film,
  HardDrive,
  Image as ImageIcon,
  Link as LinkIcon,
  Youtube,
  type LucideIcon,
} from "lucide-react";
import type { ResourceKind, WallResource } from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

export const kindIcon: Record<ResourceKind, LucideIcon> = {
  Pdf: FileText,
  Video: Film,
  Image: ImageIcon,
  Document: FileArchive,
  YouTube: Youtube,
  Drive: HardDrive,
  Link: LinkIcon,
};

export const kindLabel: Record<ResourceKind, string> = {
  Pdf: "PDF",
  Video: "Video",
  Image: "Image",
  Document: "Document",
  YouTube: "YouTube",
  Drive: "Drive",
  Link: "Link",
};

export const kindVariant: Record<ResourceKind, BadgeVariant> = {
  Pdf: "destructive",
  Video: "default",
  Image: "success",
  Document: "neutral",
  YouTube: "destructive",
  Drive: "warning",
  Link: "outline",
};

/** The kinds a viewer can filter the wall down to. */
export const kindFilters: ResourceKind[] = [
  "YouTube",
  "Video",
  "Pdf",
  "Image",
  "Document",
  "Drive",
  "Link",
];

/** Human file size. Returns null when the resource is a link and has no size. */
export function formatSize(bytes: number | null): string | null {
  if (bytes === null) return null;
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * Whether this post shows a picture rather than an icon on the wall.
 *
 * YouTube gets one for free from its thumbnail, and an uploaded image is its own preview.
 */
export function hasPreview(resource: WallResource): boolean {
  return resource.kind === "YouTube" || resource.kind === "Image";
}

/** Whether opening this post means playing something inside the platform. */
export function isPlayable(resource: WallResource): boolean {
  return resource.kind === "YouTube" || resource.kind === "Video";
}

/** The host of an external link, shown so people can see where a link goes before clicking. */
export function linkHost(url: string | null): string | null {
  if (!url) return null;
  try {
    return new URL(url).host.replace(/^www\./, "");
  } catch {
    return null;
  }
}
