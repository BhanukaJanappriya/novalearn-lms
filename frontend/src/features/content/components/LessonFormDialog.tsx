import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { FormField } from "@/components/ui/form-field";
import { Label } from "@/components/ui/label";
import { Alert } from "@/components/ui/alert";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import { useCreateLesson, useUpdateLesson } from "../api/queries";
import { LESSON_TYPES, lessonSchema, type LessonValues } from "../schemas";
import { lessonTypeLabel } from "./lessonMeta";
import type { Lesson, LessonPayload } from "../api/types";

const selectClass =
  "flex h-10 w-full rounded-md border border-input bg-card px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

const emptyValues: LessonValues = {
  title: "",
  type: "Video",
  contentUrl: "",
  textContent: "",
  durationMinutes: "",
  isPreview: false,
};

const urlPlaceholder: Record<string, string> = {
  Video: "https://videos.example.com/lesson.mp4",
  Pdf: "https://files.example.com/notes.pdf",
  Link: "https://example.com/resource",
};

/** Create or edit a lesson. Passing `lesson` switches the dialog into edit mode. */
export function LessonFormDialog({
  open,
  onClose,
  courseId,
  moduleId,
  lesson,
}: {
  open: boolean;
  onClose: () => void;
  courseId: string;
  moduleId: string;
  lesson?: Lesson | null;
}) {
  const isEdit = Boolean(lesson);
  const create = useCreateLesson(courseId);
  const update = useUpdateLesson(courseId);
  const mutation = isEdit ? update : create;

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<LessonValues>({ resolver: zodResolver(lessonSchema), defaultValues: emptyValues });

  // The form swaps its content field as the type changes.
  const type = watch("type");
  const isText = type === "Text";

  // Repopulate the form each time the dialog opens (with the lesson when editing).
  useEffect(() => {
    if (!open) return;
    create.reset();
    update.reset();
    reset(
      lesson
        ? {
            title: lesson.title,
            type: lesson.type,
            contentUrl: lesson.contentUrl ?? "",
            textContent: lesson.textContent ?? "",
            durationMinutes: lesson.durationMinutes ?? "",
            isPreview: lesson.isPreview,
          }
        : emptyValues,
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, lesson]);

  const close = () => {
    create.reset();
    update.reset();
    onClose();
  };

  const onSubmit = (values: LessonValues) => {
    const payload: LessonPayload = {
      title: values.title,
      type: values.type,
      // The server normalises this too, but sending only the relevant field keeps
      // the payload honest about what the lesson actually holds.
      contentUrl: values.type === "Text" ? null : values.contentUrl || null,
      textContent: values.type === "Text" ? values.textContent || null : null,
      durationMinutes: values.durationMinutes === "" ? null : values.durationMinutes,
      isPreview: values.isPreview,
    };

    if (lesson) {
      update.mutate({ id: lesson.id, payload }, { onSuccess: close });
    } else {
      create.mutate({ moduleId, payload }, { onSuccess: close });
    }
  };

  return (
    <Modal
      open={open}
      onClose={close}
      title={isEdit ? "Edit lesson" : "Add lesson"}
      description={isEdit ? "Update this lesson's details." : "Add a lesson to this module."}
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        {mutation.isError && <Alert>{getApiErrorMessage(mutation.error)}</Alert>}

        <FormField
          label="Title"
          placeholder="Welcome and course overview"
          error={errors.title?.message}
          {...register("title")}
        />

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <Label htmlFor="type">Type</Label>
            <select id="type" className={selectClass} {...register("type")}>
              {LESSON_TYPES.map((option) => (
                <option key={option} value={option}>
                  {lessonTypeLabel[option]}
                </option>
              ))}
            </select>
          </div>
          <FormField
            label="Duration (minutes)"
            type="number"
            min="0"
            step="1"
            placeholder="Optional"
            error={errors.durationMinutes?.message}
            {...register("durationMinutes")}
          />
        </div>

        {isText ? (
          <div className="space-y-1.5">
            <Label htmlFor="textContent">Lesson text</Label>
            <Textarea
              id="textContent"
              rows={7}
              placeholder="Write the lesson content here."
              aria-invalid={errors.textContent ? true : undefined}
              {...register("textContent")}
            />
            {errors.textContent && (
              <p className="text-xs font-medium text-destructive">{errors.textContent.message}</p>
            )}
          </div>
        ) : (
          <FormField
            label={`${lessonTypeLabel[type]} URL`}
            placeholder={urlPlaceholder[type] ?? "https://…"}
            error={errors.contentUrl?.message}
            {...register("contentUrl")}
          />
        )}

        <label className="flex items-start gap-2.5 rounded-md border border-border p-3 text-sm">
          <input
            type="checkbox"
            className="mt-0.5 h-4 w-4 rounded border-input accent-primary"
            {...register("isPreview")}
          />
          <span>
            <span className="font-medium">Free preview</span>
            <span className="mt-0.5 block text-xs text-muted-foreground">
              Anyone browsing the course can open this lesson.
            </span>
          </span>
        </label>

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={close}>
            Cancel
          </Button>
          <Button type="submit" isLoading={mutation.isPending}>
            {isEdit ? "Save changes" : "Add lesson"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
