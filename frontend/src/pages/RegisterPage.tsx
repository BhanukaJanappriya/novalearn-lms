import { useState } from "react";
import { Link } from "react-router-dom";
import { MailCheck, ShieldOff } from "lucide-react";
import { AuthLayout } from "@/layouts/AuthLayout";
import { RegisterForm } from "@/features/auth/RegisterForm";
import { Alert } from "@/components/ui/alert";
import { usePublicSettings } from "@/features/settings/api/queries";

export function RegisterPage() {
  const [registeredEmail, setRegisteredEmail] = useState<string | null>(null);
  const { data: platform } = usePublicSettings();
  const siteName = platform?.siteName ?? "NovaLearn";

  if (registeredEmail) {
    return (
      <AuthLayout
        title="Check your inbox"
        subtitle="One more step to activate your account."
        footer={
          <Link to="/login" className="font-medium text-primary hover:underline">
            Back to sign in
          </Link>
        }
      >
        <div className="space-y-4">
          <div className="flex justify-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary/10 text-primary">
              <MailCheck className="h-7 w-7" aria-hidden />
            </div>
          </div>
          <Alert variant="success">
            We sent a verification link to <strong>{registeredEmail}</strong>. Verify your email to
            sign in.
          </Alert>
          <p className="text-center text-sm text-muted-foreground">
            In local development, the link is written to the API logs.
          </p>
        </div>
      </AuthLayout>
    );
  }

  if (platform && !platform.allowNewRegistrations) {
    return (
      <AuthLayout
        title="Registration closed"
        subtitle="New accounts aren't being accepted right now."
        footer={
          <Link to="/login" className="font-medium text-primary hover:underline">
            Back to sign in
          </Link>
        }
      >
        <div className="space-y-4">
          <div className="flex justify-center">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-muted text-muted-foreground">
              <ShieldOff className="h-7 w-7" aria-hidden />
            </div>
          </div>
          <p className="text-center text-sm text-muted-foreground">
            {siteName} isn't accepting new accounts at the moment
            {platform.supportEmail && (
              <>
                {" "}
                — contact <a href={`mailto:${platform.supportEmail}`} className="text-primary hover:underline">
                  {platform.supportEmail}
                </a>{" "}
                if you need access.
              </>
            )}
          </p>
        </div>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout
      title="Create your account"
      subtitle={`Join ${siteName} and start learning today.`}
      footer={
        <>
          Already have an account?{" "}
          <Link to="/login" className="font-medium text-primary hover:underline">
            Sign in
          </Link>
        </>
      }
    >
      <RegisterForm onRegistered={setRegisteredEmail} />
    </AuthLayout>
  );
}
