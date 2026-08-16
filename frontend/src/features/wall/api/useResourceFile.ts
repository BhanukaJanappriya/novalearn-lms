import { useEffect, useState } from "react";
import { apiClient } from "@/services/apiClient";

/**
 * Fetches an uploaded file and hands back a temporary object URL for it.
 *
 * Uploads are served from an authenticated route, and the access token lives in memory rather
 * than in a cookie, so the browser cannot fetch one on its own: a plain `img src` or `a href`
 * would arrive without the Authorization header and be refused. The bytes therefore come through
 * the API client and become a blob URL.
 *
 * The URL is revoked when the component goes away, otherwise every scroll through the wall would
 * leak another copy of the file for as long as the tab is open.
 */
export function useResourceObjectUrl(resourceId: string, enabled: boolean) {
  const [objectUrl, setObjectUrl] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    if (!enabled) return;

    let cancelled = false;
    let created: string | null = null;

    setIsLoading(true);
    setFailed(false);

    apiClient
      .get<Blob>(`/resources/${resourceId}/file`, { responseType: "blob" })
      .then(({ data }) => {
        if (cancelled) return;
        created = URL.createObjectURL(data);
        setObjectUrl(created);
      })
      .catch(() => {
        if (!cancelled) setFailed(true);
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
      if (created) URL.revokeObjectURL(created);
      setObjectUrl(null);
    };
  }, [resourceId, enabled]);

  return { objectUrl, isLoading, failed };
}

/**
 * Downloads an upload under its original name.
 *
 * Same reason as above: the href cannot carry the token, so the bytes are fetched first and the
 * link is synthesised around the blob.
 */
export async function downloadResource(resourceId: string, fileName: string) {
  const { data } = await apiClient.get<Blob>(`/resources/${resourceId}/file`, {
    responseType: "blob",
  });

  const objectUrl = URL.createObjectURL(data);
  const anchor = document.createElement("a");

  anchor.href = objectUrl;
  anchor.download = fileName;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();

  URL.revokeObjectURL(objectUrl);
}
