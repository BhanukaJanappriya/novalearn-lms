import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { Alert } from "@/components/ui/alert";
import type { WallResource } from "../api/types";
import { useResourceObjectUrl } from "../api/useResourceFile";

interface ResourceViewerProps {
  resource: WallResource | null;
  onClose: () => void;
}

/**
 * Opens a post in place: a YouTube embed, or the uploaded file itself.
 *
 * Uploads are fetched only once the viewer is open, so browsing the wall never pulls down a
 * hundred megabyte video nobody asked to watch.
 */
export function ResourceViewer({ resource, onClose }: ResourceViewerProps) {
  const needsFile =
    resource !== null && resource.fileUrl !== null && resource.kind !== "Document";

  const { objectUrl, isLoading, failed } = useResourceObjectUrl(resource?.id ?? "", needsFile);

  if (!resource) return null;

  return (
    <Modal open onClose={onClose} title={resource.title} description={resource.description ?? undefined} className="max-w-3xl">
      {resource.kind === "YouTube" && resource.embedUrl && (
        <div className="aspect-video w-full overflow-hidden rounded-xl bg-black">
          <iframe
            src={resource.embedUrl}
            title={resource.title}
            className="h-full w-full"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            referrerPolicy="strict-origin-when-cross-origin"
            allowFullScreen
          />
        </div>
      )}

      {needsFile && (
        <>
          {isLoading && <Skeleton className="aspect-video w-full rounded-xl" />}
          {failed && <Alert variant="error">We could not load this file.</Alert>}

          {objectUrl && resource.kind === "Video" && (
            <video src={objectUrl} controls className="aspect-video w-full rounded-xl bg-black" />
          )}

          {objectUrl && resource.kind === "Image" && (
            <img src={objectUrl} alt={resource.title} className="max-h-[70vh] w-full rounded-xl object-contain" />
          )}

          {objectUrl && resource.kind === "Pdf" && (
            <iframe src={objectUrl} title={resource.title} className="h-[70vh] w-full rounded-xl border border-border" />
          )}
        </>
      )}

      {resource.kind === "Document" && (
        <p className="text-sm text-muted-foreground">
          This file type has no preview. Use Save to download it.
        </p>
      )}
    </Modal>
  );
}
