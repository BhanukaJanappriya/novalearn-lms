import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().min(1, "Email is required.").email("Enter a valid email address."),
  password: z.string().min(1, "Password is required."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

/** Counts digits without a regex flag dance, so the rule reads the same as the backend's. */
const digitCount = (value: string) => (value.match(/\d/g) ?? []).length;

/**
 * One source of truth for "what makes a strong password", shared by the zod schema below and the
 * live checklist under the field. Mirrors RegisterCommandValidator on the backend.
 */
export const passwordRules: { label: string; test: (value: string) => boolean }[] = [
  { label: "More than 8 characters", test: (value) => value.length >= 9 },
  { label: "An uppercase letter", test: (value) => /[A-Z]/.test(value) },
  { label: "A lowercase letter", test: (value) => /[a-z]/.test(value) },
  { label: "At least two numbers", test: (value) => digitCount(value) >= 2 },
  {
    label: "A special character (e.g. / _ - @)",
    test: (value) => /[^A-Za-z0-9]/.test(value),
  },
];

const passwordSchema = z
  .string()
  .min(9, "Password must be longer than 8 characters.")
  .max(128, "Password must be at most 128 characters.")
  .regex(/[A-Z]/, "Include at least one uppercase letter.")
  .regex(/[a-z]/, "Include at least one lowercase letter.")
  .regex(/[^A-Za-z0-9]/, "Include at least one special character (for example / _ - @).")
  .refine((value) => digitCount(value) >= 2, "Include at least two numbers.");

export const registerSchema = z
  .object({
    firstName: z.string().min(1, "First name is required.").max(100),
    lastName: z.string().min(1, "Last name is required.").max(100),
    email: z.string().min(1, "Email is required.").email("Enter a valid email address."),
    password: passwordSchema,
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
