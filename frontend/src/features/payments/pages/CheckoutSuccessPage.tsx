import { useMemo, useRef } from "react";
import { useSearchParams } from "react-router-dom";
import { CheckCircle2, Clock, Loader2 } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { LinkButton } from "@/components/ui/link-button";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { useMyEnrollments } from "@/features/enrollments/api/queries";

/** How long to keep polling before admitting the webhook is taking a while. */
const GIVE_UP_AFTER_MS = 30_000;

/**
 * Where Stripe sends the learner back after paying.
 *
 * This page never decides that payment succeeded — the redirect itself proves nothing, since a
 * learner can land here after merely closing the Checkout tab at the right moment. It polls the
 * enrolment list instead, which only ever gains a row once the webhook has confirmed payment with
 * Stripe directly, and treats that appearing as the one true signal.
 */
export function CheckoutSuccessPage() {
  const [searchParams] = useSearchParams();
  const courseId = searchParams.get("courseId");

  const startedAt = useMemo(() => Date.now(), []);
  const found = useRef(false);

  const { data: enrollments, dataUpdatedAt } = useMyEnrollments({
    // Stops polling the moment the enrolment shows up, or once it's plainly taking too long —
    // whichever comes first. A closed-over ref rather than state, so deciding to stop does not
    // itself trigger the extra render a state update would.
    refetchInterval: found.current || Date.now() - startedAt > GIVE_UP_AFTER_MS ? false : 2_000,
  });

  const enrollment = enrollments?.find((e) => e.courseId === courseId);
  found.current = Boolean(enrollment);

  const elapsed = dataUpdatedAt ? dataUpdatedAt - startedAt : 0;
  const timedOut = !enrollment && elapsed > GIVE_UP_AFTER_MS;

  return (
    <div className="min-h-screen">
      <LearnerHeader />
      <PageTransition>
        <main className="mx-auto flex max-w-lg flex-col items-center px-6 py-20 text-center">
          {enrollment ? (
            <>
              <CheckCircle2 className="h-14 w-14 text-success" aria-hidden />
              <h1 className="mt-4 text-2xl font-semibold tracking-tight">Payment confirmed</h1>
              <p className="mt-2 text-muted-foreground">
                You're enrolled in <span className="font-medium text-foreground">{enrollment.courseTitle}</span>.
              </p>
              <LinkButton to="/my-courses" className="mt-6">
                Go to my courses
              </LinkButton>
            </>
          ) : timedOut ? (
            <>
              <Clock className="h-14 w-14 text-warning" aria-hidden />
              <h1 className="mt-4 text-2xl font-semibold tracking-tight">Still confirming</h1>
              <p className="mt-2 text-muted-foreground">
                Stripe has your payment, but confirming it is taking longer than usual. Check My
                courses in a moment — it will appear there the instant it's through.
              </p>
              <LinkButton to="/my-courses" variant="outline" className="mt-6">
                Go to my courses
              </LinkButton>
            </>
          ) : (
            <>
              <Loader2 className="h-14 w-14 animate-spin text-primary" aria-hidden />
              <h1 className="mt-4 text-2xl font-semibold tracking-tight">Confirming your payment</h1>
              <p className="mt-2 text-muted-foreground">This only takes a moment.</p>
            </>
          )}
        </main>
      </PageTransition>
    </div>
  );
}
