import { useRef } from "react";
import { CreditCard } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import { getApiErrorMessage } from "@/lib/apiError";
import { useBillingOverview } from "../api/queries";
import { CurrentPlanPanel } from "../components/CurrentPlanPanel";
import { InvoiceHistoryPanel } from "../components/InvoiceHistoryPanel";
import { PaymentMethodPanel } from "../components/PaymentMethodPanel";
import { PlanCatalogue } from "../components/PlanCatalogue";
import { findPlan } from "../lib/billing";

/**
 * The billing home for an administrator: the platform's current subscription and card on file up
 * top, invoices underneath, and the plan catalogue last so "what am I on and what would change"
 * reads top to bottom.
 *
 * Everything is served from a local mock today (see {@link ../api/queries}); the plan and payment
 * actions scroll to or open the relevant area rather than mutating anything until a billing
 * provider is wired up.
 */
export function BillingPage() {
  const { data, isLoading, isError, error } = useBillingOverview();
  const catalogueRef = useRef<HTMLDivElement>(null);

  const scrollToCatalogue = () =>
    catalogueRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });

  const currentPlan = data ? findPlan(data.plans, data.subscription.planId) : undefined;

  return (
    <PageTransition>
      <div className="space-y-6">
        <header>
          <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <CreditCard className="h-6 w-6 text-primary" aria-hidden />
            Billing
          </h1>
          <p className="mt-1 text-muted-foreground">
            Your subscription, payment method and invoices, and the plans you can move to.
          </p>
        </header>

        {isError && (
          <Alert variant="error">
            {getApiErrorMessage(error, "We could not load your billing details.")}
          </Alert>
        )}

        {isLoading ? (
          <div className="space-y-6">
            <div className="grid gap-6 lg:grid-cols-2">
              <Skeleton className="h-72 rounded-[18px]" />
              <Skeleton className="h-72 rounded-[18px]" />
            </div>
            <Skeleton className="h-64 rounded-[18px]" />
            <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
              {Array.from({ length: 3 }).map((_, index) => (
                <Skeleton key={index} className="h-[28rem] rounded-[18px]" />
              ))}
            </div>
          </div>
        ) : data ? (
          <>
            <div className="grid gap-6 lg:grid-cols-2">
              {currentPlan && (
                <CurrentPlanPanel
                  subscription={data.subscription}
                  plan={currentPlan}
                  onChangePlan={scrollToCatalogue}
                />
              )}
              <PaymentMethodPanel method={data.paymentMethod} />
            </div>

            <InvoiceHistoryPanel invoices={data.invoices} />

            <div ref={catalogueRef} className="scroll-mt-6">
              <PlanCatalogue
                plans={data.plans}
                currentPlanId={data.subscription.planId}
              />
            </div>
          </>
        ) : null}
      </div>
    </PageTransition>
  );
}
