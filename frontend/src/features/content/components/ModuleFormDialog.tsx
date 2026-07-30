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
import { useCreateModule, useUpdateModule } from "../api/queries";
import { moduleSchema, type ModuleValues } from "../schemas";
import type { CourseModule, ModulePayload } from "../api/types";

const emptyValues: ModuleValues = { title: "", description: "" };

/** Create or edit a module. Passing `module` switches the dialog into edit mode. */
export function ModuleFormDialog({
  open,
  onClose,
  courseId,
  module,
}: {
  open: boolean;
  onClose: () => void;
  courseId: string;
  module?: CourseModule | null;
}) {
  const isEdit = Boolean(module);
  const create = useCreateModule(courseId);
  const update = useUpdateModule(courseId);
  const mutation = isEdit ? update : create;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ModuleValues>({ resolver: zodResolver(moduleSchema), defaultValues: emptyValues });

  // Repopulate the form each time the dialog opens (with the module when editing).
  useEffect(() => {
    if (!open) return;
    create.reset();
    update.reset();
    reset(
      module
        ? { title: module.title, description: module.description ?? "" }
        : emptyValues,
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, module]);

  const close = () => {
    create.reset();
    update.reset();
    onClose();
  };

  const onSubmit = (values: ModuleValues) => {
    const payload: ModulePayload = {
      title: values.title,
      description: values.description || null,
    };

    if (module) {
      update.mutate({ id: module.id, payload }, { onSuccess: close });
    } else {
      create.mutate(payload, { onSuccess: close });
    }
  };

  return (
    <Modal
      open={open}
      onClose={close}
      title={isEdit ? "Edit module" : "Add module"}
      description={
        isEdit ? "Update this module's details." : "Modules group the lessons of a course."
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        {mutation.isError && <Alert>{getApiErrorMessage(mutation.error)}</Alert>}

        <FormField
          label="Title"
          placeholder="Getting started"
          error={errors.title?.message}
          {...register("title")}
        />

        <div className="space-y-1.5">
          <Label htmlFor="description">Description (optional)</Label>
          <Textarea
            id="description"
            rows={3}
            placeholder="What does this module cover?"
            aria-invalid={errors.description ? true : undefined}
            {...register("description")}
          />
          {errors.description && (
            <p className="text-xs font-medium text-destructive">{errors.description.message}</p>
          )}
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={close}>
            Cancel
          </Button>
          <Button type="submit" isLoading={mutation.isPending}>
            {isEdit ? "Save changes" : "Add module"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
