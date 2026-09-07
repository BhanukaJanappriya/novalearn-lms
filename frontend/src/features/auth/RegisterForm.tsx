import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { FormField } from "@/components/ui/form-field";
import { Alert } from "@/components/ui/alert";
import { authApi } from "@/services/authApi";
import { getApiErrorMessage } from "@/lib/apiError";
import { PasswordRequirements } from "./PasswordRequirements";
import { registerSchema, type RegisterFormValues } from "./schemas";

export function RegisterForm({ onRegistered }: { onRegistered: (email: string) => void }) {
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { acceptTerms: false },
  });

  const passwordValue = watch("password") ?? "";

  const mutation = useMutation({
    mutationFn: (values: RegisterFormValues) =>
      authApi.register({
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
        acceptedTerms: values.acceptTerms,
      }),
    onSuccess: (_data, values) => onRegistered(values.email),
  });

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))} className="space-y-4" noValidate>
      {mutation.isError && <Alert>{getApiErrorMessage(mutation.error)}</Alert>}

      <div className="grid grid-cols-2 gap-3">
        <FormField
          label="First name"
          autoComplete="given-name"
          error={errors.firstName?.message}
          {...register("firstName")}
        />
        <FormField
          label="Last name"
          autoComplete="family-name"
          error={errors.lastName?.message}
          {...register("lastName")}
        />
      </div>

      <FormField
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="you@university.edu"
        error={errors.email?.message}
        {...register("email")}
      />

      <div>
        <FormField
          label="Password"
          type="password"
          autoComplete="new-password"
          error={errors.password?.message}
          {...register("password")}
        />
        <PasswordRequirements value={passwordValue} />
      </div>

      <FormField
        label="Confirm password"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register("confirmPassword")}
      />

      <div>
        <label className="flex items-start gap-2.5 text-sm text-muted-foreground">
          <Checkbox className="mt-0.5" {...register("acceptTerms")} />
          <span>
            I agree to the{" "}
            <Link
              to="/terms"
              target="_blank"
              rel="noreferrer"
              className="font-medium text-primary hover:underline"
            >
              Terms of Service
            </Link>{" "}
            and{" "}
            <Link
              to="/privacy"
              target="_blank"
              rel="noreferrer"
              className="font-medium text-primary hover:underline"
            >
              Privacy Policy
            </Link>
            .
          </span>
        </label>
        {errors.acceptTerms && (
          <p className="mt-1 text-xs font-medium text-destructive">{errors.acceptTerms.message}</p>
        )}
      </div>

      <Button type="submit" className="w-full" isLoading={mutation.isPending}>
        Create account
      </Button>
    </form>
  );
}
