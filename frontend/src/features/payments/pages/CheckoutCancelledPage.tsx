import { XCircle } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { LinkButton } from "@/components/ui/link-button";
import { LearnerHeader } from "@/layouts/LearnerHeader";

/** Where Stripe sends the learner back if they leave Checkout without paying. Nothing was charged. */
export function CheckoutCancelledPage() {
  return (
    <div className="min-h-screen">
      <LearnerHeader />
      <PageTransition>
        <main className="mx-auto flex max-w-lg flex-col items-center px-6 py-20 text-center">
          <XCircle className="h-14 w-14 text-muted-foreground" aria-hidden />
          <h1 className="mt-4 text-2xl font-semibold tracking-tight">Checkout cancelled</h1>
          <p className="mt-2 text-muted-foreground">
            Nothing was charged. You can try again whenever you're ready.
          </p>
          <LinkButton to="/catalog" className="mt-6">
            Back to catalog
          </LinkButton>
        </main>
      </PageTransition>
    </div>
  );
}
