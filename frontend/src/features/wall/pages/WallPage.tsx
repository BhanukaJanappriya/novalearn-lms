import { useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { LayoutGrid, Plus, Search, TriangleAlert } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { Skeleton } from "@/components/ui/skeleton";
import { useAuth } from "@/context/AuthContext";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { staggerContainer } from "@/lib/motion";
import { useDeleteResource, useWall } from "../api/queries";
import type { ResourceKind, WallResource } from "../api/types";
import { PostResourceDialog } from "../components/PostResourceDialog";
import { ResourceCard } from "../components/ResourceCard";
import { ResourceViewer } from "../components/ResourceViewer";
import { kindFilters, kindLabel } from "../lib/resources";

/**
 * The platform wall: notes, videos and links posted by teaching staff.
 *
 * Everyone signed in reads it. What each person sees is decided by the server per post, not by
 * hiding controls here: a post attached to a course reaches that course's members only.
 */
export function WallPage() {
  const { user } = useAuth();
  const [kind, setKind] = useState<ResourceKind | undefined>();
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search, 300);

  const [posting, setPosting] = useState(false);
  const [viewing, setViewing] = useState<WallResource | null>(null);
  const [pendingDelete, setPendingDelete] = useState<WallResource | null>(null);

  const filters = useMemo(
    () => ({ kind, search: debouncedSearch }),
    [kind, debouncedSearch],
  );

  const { data: wall, isLoading, isError, error } = useWall(filters);
  const deleteResource = useDeleteResource();

  const canPost = user?.roles.some(
    (role) => role === "Lecturer" || role === "Administrator" || role === "SuperAdministrator",
  );

  const confirmDelete = () => {
    if (!pendingDelete) return;
    deleteResource.mutate(pendingDelete.id, { onSuccess: () => setPendingDelete(null) });
  };

  return (
    <PageTransition>
      <div className="space-y-6">
        <header className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
              <LayoutGrid className="h-6 w-6 text-primary" aria-hidden />
              Wall
            </h1>
            <p className="mt-1 text-muted-foreground">
              Notes, recordings and links shared across the platform.
            </p>
          </div>

          {canPost && (
            <Button size="sm" onClick={() => setPosting(true)}>
              <Plus className="h-4 w-4" />
              Add content
            </Button>
          )}
        </header>

        <div className="flex flex-wrap items-center gap-3">
          <div className="flex flex-wrap gap-1.5">
            <button
              type="button"
              onClick={() => setKind(undefined)}
              aria-pressed={kind === undefined}
              className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                kind === undefined
                  ? "bg-primary/10 text-primary"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
              }`}
            >
              Everything
            </button>
            {kindFilters.map((option) => (
              <button
                key={option}
                type="button"
                onClick={() => setKind(option)}
                aria-pressed={kind === option}
                className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                  kind === option
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {kindLabel[option]}
              </button>
            ))}
          </div>

          <div className="relative min-w-[220px] flex-1">
            <Search
              className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
              aria-hidden
            />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search the wall"
              aria-label="Search the wall"
              className="pl-9"
            />
          </div>

          {wall && <Badge variant="neutral">{wall.length} posts</Badge>}
        </div>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load the wall.")}</Alert>
        )}

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, index) => (
              <Skeleton key={index} className="h-72 rounded-[18px]" />
            ))}
          </div>
        ) : wall && wall.length === 0 ? (
          <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
            <p className="font-medium">Nothing here yet.</p>
            <p className="mt-1 text-sm text-muted-foreground">
              {canPost
                ? "Add a link or upload a file to get started."
                : "Material your lecturers share will appear here."}
            </p>
          </div>
        ) : wall ? (
          <motion.div
            className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
            initial="hidden"
            animate="visible"
            variants={staggerContainer}
          >
            <AnimatePresence mode="popLayout">
              {wall.map((resource) => (
                <ResourceCard
                  key={resource.id}
                  resource={resource}
                  onOpen={setViewing}
                  onDelete={setPendingDelete}
                />
              ))}
            </AnimatePresence>
          </motion.div>
        ) : null}
      </div>

      {canPost && (
        <PostResourceDialog open={posting} onClose={() => setPosting(false)} />
      )}

      <ResourceViewer resource={viewing} onClose={() => setViewing(null)} />

      <Modal
        open={pendingDelete !== null}
        onClose={() => setPendingDelete(null)}
        title="Remove this post?"
        className="max-w-md"
      >
        <div className="flex items-start gap-3">
          <TriangleAlert className="mt-0.5 h-5 w-5 shrink-0 text-destructive" aria-hidden />
          <p className="text-sm text-muted-foreground">
            <span className="font-medium text-foreground">{pendingDelete?.title}</span> will be
            taken off the wall. Anyone relying on it will lose access.
          </p>
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <Button variant="ghost" onClick={() => setPendingDelete(null)}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={confirmDelete}
            isLoading={deleteResource.isPending}
          >
            Remove
          </Button>
        </div>
      </Modal>
    </PageTransition>
  );
}
