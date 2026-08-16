export type ResourceKind =
  | "Pdf"
  | "Video"
  | "Image"
  | "Document"
  | "YouTube"
  | "Drive"
  | "Link";

/** Mirrors the backend `ResourceDto`. */
export interface WallResource {
  id: string;
  title: string;
  description: string | null;
  kind: ResourceKind;
  /** Where an external resource lives. Null for uploads. */
  url: string | null;
  /** API route serving an upload's bytes. Null for links. */
  fileUrl: string | null;
  originalFileName: string | null;
  sizeBytes: number | null;
  /** Present for YouTube posts only, and what the wall shows as the card image. */
  thumbnailUrl: string | null;
  embedUrl: string | null;
  courseId: string | null;
  courseTitle: string | null;
  postedById: string;
  postedByName: string;
  postedByAvatarUrl: string | null;
  postedAtUtc: string;
  /** Whether the signed-in viewer may remove this post. Decided by the server. */
  canManage: boolean;
}

export interface PostLinkInput {
  title: string;
  description?: string | null;
  url: string;
  courseId?: string | null;
}

export interface UploadInput {
  title: string;
  description?: string | null;
  courseId?: string | null;
  file: File;
}

export interface WallFilters {
  courseId?: string;
  kind?: ResourceKind;
  search?: string;
}
