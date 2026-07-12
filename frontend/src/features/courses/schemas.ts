import { z } from "zod";

export const createCourseSchema = z.object({
  title: z.string().min(1, "Title is required").max(200),
  code: z
    .string()
    .min(1, "Code is required")
    .max(20)
    .regex(/^[A-Za-z0-9- ]+$/, "Letters, numbers, spaces and hyphens only"),
  category: z.string().min(1, "Category is required").max(100),
  level: z.enum(["Beginner", "Intermediate", "Advanced"]),
  status: z.enum(["Draft", "Published"]),
  price: z.coerce.number({ invalid_type_error: "Enter a number" }).min(0, "Price cannot be negative"),
  description: z.string().max(2000).optional().or(z.literal("")),
  coverImageUrl: z.string().url("Must be a valid URL").max(512).optional().or(z.literal("")),
});

export type CreateCourseValues = z.infer<typeof createCourseSchema>;
