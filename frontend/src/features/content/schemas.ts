import { z } from "zod";

export const moduleSchema = z.object({
  title: z.string().min(1, "Title is required").max(200),
  description: z.string().max(2000).optional().or(z.literal("")),
});

export type ModuleValues = z.infer<typeof moduleSchema>;

export const LESSON_TYPES = ["Video", "Pdf", "Text", "Link"] as const;

export const lessonSchema = z
  .object({
    title: z.string().min(1, "Title is required").max(200),
    type: z.enum(LESSON_TYPES),
    contentUrl: z.string().max(1024).optional().or(z.literal("")),
    textContent: z.string().max(20000).optional().or(z.literal("")),
    // An empty field means "no estimate", so blank is allowed alongside a whole number.
    durationMinutes: z.union([
      z.literal(""),
      z.coerce
        .number({ invalid_type_error: "Enter a number" })
        .int("Whole minutes only")
        .min(0, "Duration cannot be negative")
        .max(100000),
    ]),
    isPreview: z.boolean(),
  })
  .superRefine((values, ctx) => {
    // A text lesson carries its body inline; every other type points at a resource.
    if (values.type === "Text") {
      if (!values.textContent?.trim()) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["textContent"],
          message: "Lesson text is required",
        });
      }
      return;
    }

    if (!values.contentUrl?.trim()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["contentUrl"],
        message: "A URL is required for this lesson type",
      });
    }
  });

export type LessonValues = z.infer<typeof lessonSchema>;
