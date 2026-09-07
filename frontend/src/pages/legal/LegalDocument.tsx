import { Link } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { Logo } from "@/components/Logo";
import { ThemeToggle } from "@/components/ThemeToggle";

/**
 * Shell for the standalone legal pages (Terms, Privacy). Deliberately outside the app shell and
 * the auth layout: these open in their own tab from the sign-up form and from public footers, so
 * they need to stand on their own.
 *
 * The version date shown here is the same string the backend stores against each account when it
 * accepts the terms (NovaLearn.Domain.Identity.TermsAgreement.CurrentVersion). Keep them in step.
 */
export function LegalDocument({
  title,
  version,
  children,
}: {
  title: string;
  version: string;
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen bg-background">
      <header className="flex items-center justify-between border-b border-border px-6 py-4">
        <Link to="/" aria-label="Home">
          <Logo />
        </Link>
        <ThemeToggle />
      </header>

      <main className="mx-auto max-w-3xl px-6 py-10">
        <Link
          to="/register"
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to sign up
        </Link>

        <h1 className="mt-6 text-3xl font-semibold tracking-tight">{title}</h1>
        <p className="mt-1 text-sm text-muted-foreground">Version {version}</p>

        <div className="mt-8 space-y-6 text-sm leading-relaxed text-foreground [&_h2]:text-lg [&_h2]:font-semibold [&_h2]:tracking-tight [&_p]:text-muted-foreground [&_li]:text-muted-foreground [&_ul]:list-disc [&_ul]:space-y-1 [&_ul]:pl-5">
          {children}
        </div>
      </main>
    </div>
  );
}
