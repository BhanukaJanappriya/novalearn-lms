import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { FormField } from "@/components/ui/form-field";
import { Label } from "@/components/ui/label";
import { Alert } from "@/components/ui/alert";
import { Modal } from "@/components/ui/modal";
import { cn } from "@/lib/utils";
import { getApiErrorMessage } from "@/lib/apiError";
import { useCreateCourse } from "../api/queries";
import { createCourseSchema, type CreateCourseValues } from "../schemas";

const selectClass =
  "flex h-10 w-full rounded-md border border-input bg-card px-3 py-2 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

export function CreateCourseDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const create = useCreateCourse();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateCourseValues>({
    resolver: zodResolver(createCourseSchema),
    defaultValues: { level: "Beginner", status: "Draft", price: 0 },
  });

  const close = () => {
    reset();
    create.reset();
    onClose();
  };

  const onSubmit = (values: CreateCourseValues) => {
    create.mutate(
      {
        title: values.title,
        code: values.code,
        category: values.category,
        level: values.level,
        status: values.status,
        price: values.price,
        description: values.description || null,
        coverImageUrl: values.coverImageUrl || null,
      },
      { onSuccess: close },
    );
  };

  return (
    <Modal open={open} onClose={close} title="Create course" description="Add a new course to the catalogue.">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        {create.isError && <Alert>{getApiErrorMessage(create.error)}</Alert>}

        <FormField label="Title" placeholder="Intro to Programming" error={errors.title?.message} {...register("title")} />

        <div className="grid grid-cols-2 gap-3">
          <FormField label="Code" placeholder="CS101" error={errors.code?.message} {...register("code")} />
          <FormField label="Category" placeholder="Computer Science" error={errors.category?.message} {...register("category")} />
        </div>

        <div className="grid grid-cols-3 gap-3">
          <div className="space-y-1.5">
            <Label htmlFor="level">Level</Label>
            <select id="level" className={selectClass} {...register("level")}>
              <option value="Beginner">Beginner</option>
              <option value="Intermediate">Intermediate</option>
              <option value="Advanced">Advanced</option>
            </select>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="status">Status</Label>
            <select id="status" className={selectClass} {...register("status")}>
              <option value="Draft">Draft</option>
              <option value="Published">Published</option>
            </select>
          </div>
          <FormField
            label="Price (USD)"
            type="number"
            step="0.01"
            min="0"
            error={errors.price?.message}
            {...register("price")}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="description">Description</Label>
          <textarea
            id="description"
            rows={3}
            placeholder="What will students learn?"
            className={cn(selectClass, "h-auto resize-none")}
            aria-invalid={errors.description ? true : undefined}
            {...register("description")}
          />
          {errors.description && <p className="text-xs font-medium text-destructive">{errors.description.message}</p>}
        </div>

        <FormField
          label="Cover image URL (optional)"
          placeholder="https://…"
          error={errors.coverImageUrl?.message}
          {...register("coverImageUrl")}
        />

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={close}>
            Cancel
          </Button>
          <Button type="submit" isLoading={create.isPending}>
            Create course
          </Button>
        </div>
      </form>
    </Modal>
  );
}
