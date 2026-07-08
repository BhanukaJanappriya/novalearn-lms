import { useEffect, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { Loader2 } from "lucide-react";
import { AuthLayout } from "@/layouts/AuthLayout";
import { Alert } from "@/components/ui/alert";
import { authApi } from "@/services/authApi";
import { getApiErrorMessage } from "@/lib/apiError";

type Status = "verifying" | "success" | "error";

export function VerifyEmailPage() {
  const [params] = useSearchParams();
  const [status, setStatus] = useState<Status>("verifying");
  const [message, setMessage] = useState("");
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return; // guard against double-invoke in StrictMode
    started.current = true;

    const userId = params.get("userId");
    const token = params.get("token");

    if (!userId || !token) {
      setStatus("error");
      setMessage("This verification link is missing required information.");
      return;
    }

    void authApi
      .verifyEmail(userId, token)
      .then(() => setStatus("success"))
      .catch((error: unknown) => {
        setStatus("error");
        setMessage(getApiErrorMessage(error, "The verification link is invalid or has expired."));
      });
  }, [params]);

  return (
    <AuthLayout
      title="Email verification"
      subtitle="Confirming your email address."
      footer={
        <Link to="/login" className="font-medium text-primary hover:underline">
          Continue to sign in
        </Link>
      }
    >
      {status === "verifying" && (
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
          Verifying your email…
        </div>
      )}
      {status === "success" && (
        <Alert variant="success">Your email is verified. You can now sign in.</Alert>
      )}
      {status === "error" && <Alert>{message}</Alert>}
    </AuthLayout>
  );
}
