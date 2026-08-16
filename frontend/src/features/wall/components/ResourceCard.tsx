import { useState } from "react";
import { motion } from "framer-motion";
import { Download, ExternalLink, Play, Trash2 } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { timeAgo } from "@/lib/format";
import { staggerItem } from "@/lib/motion";
import { avatarColor } from "@/features/users/lib/userVisuals";
import type { WallResource } from "../api/types";
import { useResourceObjectUrl, downloadResource } from "../api/useResourceFile";
import { formatSize, hasPreview, isPlayable, kindIcon, kindLabel, kindVariant, linkHost } from "../lib/resources";

interface ResourceCardProps {
  resource: WallResource;
  onOpen: (resource: WallResource) => void;
  onDelete: (resource: WallResource) => void;
}

/** One post on the wall. */
export function ResourceCard({ resource, onOpen, onDelete }: ResourceCardProps) {
  const Icon = kindIcon[resource.kind];
  const [downloading, setDownloading] = useState(false);

  // An uploaded image is its own preview, so its bytes are fetched for the card. Everything else
  // either has a free thumbnail (YouTube) or shows an icon, so nothing large is pulled down just
  // to render the wall.
  const { objectUrl } = useResourceObjectUrl(resource.id, resource.kind === "Image");

  const previewSrc = resource.kind === "YouTube" ? resource.thumbnailUrl : objectUrl;
  const size = formatSize(resource.sizeBytes);
  const host = linkHost(resource.url);

  const download = async () => {
    setDownloading(true);
    try {
      await downloadResource(resource.id, resource.originalFileName ?? resource.title);
    } finally {
      setDownloading(false);
    }
  };

  return (
    <motion.article
      layout
      variants={staggerItem}
      exit={{ opacity: 0, scale: 0.96, transition: { duration: 0.18 } }}
      whileHover={{ y: -4 }}
      transition={{ type: "spring", stiffness: 320, damping: 24 }}
      className="flex flex-col overflow-hidden rounded-[18px] border border-border bg-card shadow-soft"
    >
      {hasPreview(resource) ? (
        <button
          type="button"
          onClick={() => onOpen(resource)}
          className="group relative block aspect-video w-full overflow-hidden bg-muted"
          aria-label={`Open ${resource.title}`}
        >
          {previewSrc ? (
            <img
              src={previewSrc}
              alt=""
              loading="lazy"
              className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
            />
          ) : (
            <span className="flex h-full w-full items-center justify-center">
              <Icon className="h-8 w-8 text-muted-foreground" aria-hidden />
            </span>
          )}

          {isPlayable(resource) && (
            <span className="absolute inset-0 flex items-center justify-center bg-black/25 transition-colors group-hover:bg-black/35">
              <span className="grid h-12 w-12 place-items-center rounded-full bg-white/90 shadow-lg transition-transform group-hover:scale-110">
                <Play className="ml-0.5 h-5 w-5 fill-current text-foreground" aria-hidden />
              </span>
            </span>
          )}
        </button>
      ) : (
        <div className="flex aspect-[3/1] w-full items-center justify-center bg-muted/60">
          <Icon className="h-9 w-9 text-muted-foreground" aria-hidden />
        </div>
      )}

      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-start justify-between gap-2">
          <h2 className="min-w-0 font-semibold leading-snug">{resource.title}</h2>
          <Badge variant={kindVariant[resource.kind]}>{kindLabel[resource.kind]}</Badge>
        </div>

        {resource.description && (
          <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{resource.description}</p>
        )}

        <div className="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-muted-foreground">
          {resource.courseTitle ? (
            <Badge variant="outline">{resource.courseTitle}</Badge>
          ) : (
            <Badge variant="neutral">Everyone</Badge>
          )}
          {size && <span>{size}</span>}
          {host && <span className="truncate">{host}</span>}
        </div>

        <div className="mt-3 flex items-center gap-2">
          <Avatar
            name={resource.postedByName}
            src={resource.postedByAvatarUrl}
            color={avatarColor(resource.postedById)}
            size="sm"
          />
          <p className="min-w-0 flex-1 truncate text-xs text-muted-foreground">
            {resource.postedByName} · {timeAgo(resource.postedAtUtc)}
          </p>
        </div>

        <div className="mt-auto flex items-center gap-1 pt-4">
          {resource.fileUrl ? (
            <>
              <Button variant="outline" size="sm" onClick={() => onOpen(resource)}>
                Open
              </Button>
              <Button variant="ghost" size="sm" onClick={download} isLoading={downloading}>
                <Download className="h-3.5 w-3.5" />
                Save
              </Button>
            </>
          ) : isPlayable(resource) ? (
            <Button variant="outline" size="sm" onClick={() => onOpen(resource)}>
              <Play className="h-3.5 w-3.5" />
              Play
            </Button>
          ) : (
            <a
              href={resource.url ?? "#"}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-1.5 rounded-lg border border-border px-3 py-1.5 text-xs font-medium hover:bg-muted"
            >
              <ExternalLink className="h-3.5 w-3.5" aria-hidden />
              Open link
            </a>
          )}

          {resource.canManage && (
            <Button
              variant="ghost"
              size="sm"
              className="ml-auto text-destructive"
              onClick={() => onDelete(resource)}
              aria-label={`Remove ${resource.title}`}
            >
              <Trash2 className="h-3.5 w-3.5" />
            </Button>
          )}
        </div>
      </div>
    </motion.article>
  );
}
