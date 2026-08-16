import { useState } from "react";
import { Link2, Upload } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Progress } from "@/components/ui/progress";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import { useCourses } from "@/features/courses/api/queries";
import { usePostLink, useUploadResource } from "../api/queries";
import { ACCEPTED_EXTENSIONS, MAX_UPLOAD_MB } from "../lib/uploads";

interface PostResourceDialogProps {
  open: boolean;
  onClose: () => void;
}

type Mode = "link" | "upload";

/** Staff-only dialog for adding material to the wall, either a link or a file. */
export function PostResourceDialog({ open, onClose }: PostResourceDialogProps) {
  // Fetched here rather than by the page, so only somebody who can actually post asks for it.
  const { data: courses } = useCourses();

  const [mode, setMode] = useState<Mode>("link");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [url, setUrl] = useState("");
  const [courseId, setCourseId] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [sizeError, setSizeError] = useState<string | null>(null);

  const postLink = usePostLink();
  const upload = useUploadResource();

  const busy = postLink.isPending || upload.isPending;
  const error = postLink.error ?? upload.error;

  const reset = () => {
    setTitle("");
    setDescription("");
    setUrl("");
    setCourseId("");
    setFile(null);
    setSizeError(null);
    postLink.reset();
    upload.reset();
  };

  const close = () => {
    reset();
    onClose();
  };

  const chooseFile = (chosen: File | null) => {
    setSizeError(null);

    if (chosen && chosen.size > MAX_UPLOAD_MB * 1024 * 1024) {
      setSizeError(`That file is larger than the ${MAX_UPLOAD_MB} MB limit.`);
      setFile(null);
      return;
    }

    setFile(chosen);

    // Saves retyping what the file is already called.
    if (chosen && !title.trim()) {
      setTitle(chosen.name.replace(/\.[^.]+$/, ""));
    }
  };

  const submit = () => {
    const shared = {
      title: title.trim(),
      description: description.trim() || null,
      courseId: courseId || null,
    };

    if (mode === "link") {
      postLink.mutate({ ...shared, url: url.trim() }, { onSuccess: close });
      return;
    }

    if (file) {
      upload.mutate({ ...shared, file }, { onSuccess: close });
    }
  };

  const canSubmit =
    title.trim().length > 0 && (mode === "link" ? url.trim().length > 0 : file !== null);

  return (
    <Modal open={open} onClose={close} title="Add to the wall" className="max-w-lg">
      <div className="flex gap-1.5">
        {(
          [
            { id: "link", label: "Paste a link", icon: Link2 },
            { id: "upload", label: "Upload a file", icon: Upload },
          ] as const
        ).map((option) => (
          <button
            key={option.id}
            type="button"
            onClick={() => setMode(option.id)}
            aria-pressed={mode === option.id}
            className={`inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
              mode === option.id
                ? "bg-primary/10 text-primary"
                : "text-muted-foreground hover:bg-muted hover:text-foreground"
            }`}
          >
            <option.icon className="h-3.5 w-3.5" aria-hidden />
            {option.label}
          </button>
        ))}
      </div>

      {error && (
        <Alert variant="error" className="mt-4">
          {getApiErrorMessage(error)}
        </Alert>
      )}
      {sizeError && (
        <Alert variant="error" className="mt-4">
          {sizeError}
        </Alert>
      )}

      <div className="mt-4 space-y-4">
        {mode === "link" ? (
          <div className="space-y-1.5">
            <Label htmlFor="resource-url">Address</Label>
            <Input
              id="resource-url"
              value={url}
              onChange={(event) => setUrl(event.target.value)}
              placeholder="https://www.youtube.com/watch?v=..."
              maxLength={2048}
            />
            <p className="text-xs text-muted-foreground">
              YouTube and Google Drive addresses are recognised automatically. A YouTube link shows
              its thumbnail on the wall.
            </p>
          </div>
        ) : (
          <div className="space-y-1.5">
            <Label htmlFor="resource-file">File</Label>
            <Input
              id="resource-file"
              type="file"
              accept={ACCEPTED_EXTENSIONS}
              onChange={(event) => chooseFile(event.target.files?.[0] ?? null)}
              className="file:mr-3 file:rounded-md file:border-0 file:bg-muted file:px-3 file:py-1 file:text-xs file:font-medium"
            />
            <p className="text-xs text-muted-foreground">
              PDFs, videos, images and documents, up to {MAX_UPLOAD_MB} MB.
            </p>
          </div>
        )}

        <div className="space-y-1.5">
          <Label htmlFor="resource-title">Title</Label>
          <Input
            id="resource-title"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            placeholder="Week 1 lecture notes"
            maxLength={200}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="resource-description">Description</Label>
          <Textarea
            id="resource-description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            placeholder="What is this, and who is it for?"
            rows={3}
            maxLength={2000}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="resource-course">Post to</Label>
          <select
            id="resource-course"
            value={courseId}
            onChange={(event) => setCourseId(event.target.value)}
            className="h-10 w-full rounded-lg border border-border bg-background px-3 text-sm"
          >
            <option value="">Everyone on the platform</option>
            {(courses ?? []).map((course) => (
              <option key={course.id} value={course.id}>
                {course.title}
              </option>
            ))}
          </select>
          <p className="text-xs text-muted-foreground">
            A course post is only visible to people on that course.
          </p>
        </div>

        {upload.isPending && upload.progress > 0 && (
          <div>
            <div className="mb-1 flex justify-between text-xs text-muted-foreground">
              <span>Uploading</span>
              <span className="tabular-nums">{upload.progress}%</span>
            </div>
            <Progress value={upload.progress} label="Upload" size="sm" />
          </div>
        )}
      </div>

      <div className="mt-6 flex justify-end gap-2">
        <Button variant="ghost" onClick={close} disabled={busy}>
          Cancel
        </Button>
        <Button onClick={submit} isLoading={busy} disabled={!canSubmit}>
          Post
        </Button>
      </div>
    </Modal>
  );
}
