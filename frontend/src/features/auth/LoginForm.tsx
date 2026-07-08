import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { FormField } from "@/components/ui/form-field";
import { Alert } from "@/components/ui/alert";
import { authApi } from "@/services/authApi";
import { getApiErrorMessage } from "@/lib/apiError";
import { useAuth } from "@/context/AuthContext";
import { loginSchema, type LoginFormValues } from "./schemas";
import type { AuthenticationResponse } from "@/types/auth";

export function LoginForm() {
  const navigate = useNavigate();
  const { setSession } = useAuth();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const mutation = useMutation({
    mutationFn: (values: LoginFormValues) => authApi.login(values),
    onSuccess: (data: AuthenticationResponse) => {
      setSession(data.user);
      navigate("/dashboard", { replace: true });
    },
  });

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))} className="space-y-4" noValidate>
      {mutation.isError && <Alert>{getApiErrorMessage(mutation.error)}</Alert>}

      <FormField
        label="Email"
        type="email"
        autoComplete="email"
        placeholder="you@university.edu"
        error={errors.email?.message}
        {...register("email")}
      />

      <FormField
        label="Password"
        type="password"
        autoComplete="current-password"
        placeholder="••••••••"
        error={errors.password?.message}
        {...register("password")}
      />

      <Button type="submit" className="w-full" isLoading={mutation.isPending}>
        Sign in
      </Button>
    </form>
  );
}
