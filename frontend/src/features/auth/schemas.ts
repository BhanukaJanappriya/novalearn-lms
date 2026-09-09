import { z } from "zod";
import { strongPasswordSchema } from "@/lib/passwordPolicy";

export const loginSchema = z.object({
  email: z.string().min(1, "Email is required.").email("Enter a valid email address."),
  password: z.string().min(1, "Password is required."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    firstName: z.string().min(1, "First name is required.").max(100),
    lastName: z.string().min(1, "Last name is required.").max(100),
    email: z.string().min(1, "Email is required.").email("Enter a valid email address."),
    password: strongPasswordSchema,
    confirmPassword: z.string(),
    acceptTerms: z.boolean().refine((accepted) => accepted, {
      message: "You must accept the Terms of Service and Privacy Policy to continue.",
    }),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

export type RegisterFormValues = z.infer<typeof registerSchema>;
