import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { FormField } from "@/components/ui/form-field";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { PasswordRequirements } from "@/components/PasswordRequirements";
import { getApiErrorMessage } from "@/lib/apiError";
import { strongPasswordSchema } from "@/lib/passwordPolicy";
import type { CreateUserInput } from "../api/types";

const schema = z.object({
  firstName: z.string().min(1, "First name is required.").max(100),
  lastName: z.string().min(1, "Last name is required.").max(100),
  email: z.string().min(1, "Email is required.").email("Enter a valid email address."),
  role: z.string().min(1, "Choose a role."),
  password: strongPasswordSchema,
});

type FormValues = z.infer<typeof schema>;

interface AddUserDialogProps {
  open: boolean;
  onClose: () => void;
  /** Every assignable role, from GET /admin/roles. */
  availableRoles: string[];
  /** Roles the signed-in admin cannot assign, e.g. SuperAdministrator for a non-super admin. */
  lockedRoles: string[];
  onSubmit: (input: CreateUserInput) => void;
  isSubmitting: boolean;
  error: unknown;
}

/**
 * Lets an administrator add an account directly. The new account is created with its email
 * already confirmed and can sign in immediately with the password set here; the server enforces
 * the same password policy and role rules regardless of what this form allows.
 */
export function AddUserDialog({
  open,
  onClose,
  availableRoles,
  lockedRoles,
  onSubmit,
  isSubmitting,
  error,
}: AddUserDialogProps) {
  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { role: "" } });

  // Start clean each time the dialog opens.
  useEffect(() => {
    if (open) reset({ firstName: "", lastName: "", email: "", role: "", password: "" });
  }, [open, reset]);

  const password = watch("password") ?? "";
  const roleOptions = availableRoles.filter((role) => !lockedRoles.includes(role));

  return (
    <Modal open={open} onClose={onClose} title="Add user" description="Create a lecturer, student or other account.">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        {error ? <Alert variant="error">{getApiErrorMessage(error, "We could not create the account.")}</Alert> : null}

        <div className="grid grid-cols-2 gap-3">
          <FormField
            label="First name"
            autoComplete="off"
            error={errors.firstName?.message}
            {...register("firstName")}
          />
          <FormField
            label="Last name"
            autoComplete="off"
            error={errors.lastName?.message}
            {...register("lastName")}
          />
        </div>

        <FormField
          label="Email"
          type="email"
          autoComplete="off"
          placeholder="name@university.edu"
          error={errors.email?.message}
          {...register("email")}
        />

        <div className="space-y-1.5">
          <Label htmlFor="add-user-role">Role</Label>
          <select
            id="add-user-role"
            aria-invalid={errors.role ? true : undefined}
            className="h-10 w-full rounded-lg border border-border bg-card px-3 text-sm outline-none ring-offset-background focus-visible:ring-2 focus-visible:ring-ring"
            {...register("role")}
          >
            <option value="">Select a role…</option>
            {roleOptions.map((role) => (
              <option key={role} value={role}>
                {role}
              </option>
            ))}
          </select>
          {errors.role && <p className="text-xs font-medium text-destructive">{errors.role.message}</p>}
        </div>

        <div>
          <FormField
            label="Temporary password"
            type="password"
            autoComplete="new-password"
            error={errors.password?.message}
            {...register("password")}
          />
          <PasswordRequirements value={password} />
          <p className="mt-1.5 text-xs text-muted-foreground">
            Share this with the person; they can change it after signing in.
          </p>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            Add user
          </Button>
        </div>
      </form>
    </Modal>
  );
}
