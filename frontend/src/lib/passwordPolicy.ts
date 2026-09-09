import { z } from "zod";

/**
 * The account password policy, in one place on the client. Used by self-registration and by an
 * administrator adding an account, and mirrored on the server in
 * NovaLearn.Application/Common/Validation/PasswordRules.cs, which is the authoritative gate.
 */

/** Counts digits without a regex flag dance, so the rule reads the same as the backend's. */
const digitCount = (value: string) => (value.match(/\d/g) ?? []).length;

/** Each rule as a label plus a predicate, for the live checklist under a password field. */
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

/** The same policy as a zod schema, for form validation. */
export const strongPasswordSchema = z
  .string()
  .min(9, "Password must be longer than 8 characters.")
  .max(128, "Password must be at most 128 characters.")
  .regex(/[A-Z]/, "Include at least one uppercase letter.")
  .regex(/[a-z]/, "Include at least one lowercase letter.")
  .regex(/[^A-Za-z0-9]/, "Include at least one special character (for example / _ - @).")
  .refine((value) => digitCount(value) >= 2, "Include at least two numbers.");
