/**
 * Upload limits as the client understands them.
 *
 * These mirror the server, which is the one that actually enforces them. Keeping a copy here is
 * about failing fast and politely, before a large file is pushed up a slow connection only to be
 * refused on arrival. If the two ever disagree, the server wins.
 */
export const MAX_UPLOAD_MB = 200;

/** Matches the backend allowlist in `UploadedFileTypes`. */
export const ACCEPTED_EXTENSIONS = [
  ".pdf",
  ".mp4",
  ".webm",
  ".mov",
  ".m4v",
  ".png",
  ".jpg",
  ".jpeg",
  ".gif",
  ".webp",
  ".doc",
  ".docx",
  ".ppt",
  ".pptx",
  ".xls",
  ".xlsx",
  ".txt",
  ".md",
  ".csv",
  ".zip",
].join(",");
