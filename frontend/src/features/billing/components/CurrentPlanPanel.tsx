import { AlertTriangle, BadgeCheck, CalendarClock, Info, Receipt } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import type { CurrentSubscription, Plan } from "../api/types";
import {
  formatDate,
  formatMoney,
  subscriptionStatusLabel,
  subscriptionStatusVariant,
} from "../lib/billing";

interface CurrentPlanPanelProps {
  subscription: CurrentSubscription;
  plan: Plan;
  onChangePlan?: () => void;
  onCancel?: () => void;
}

/**
 * The platform's current subscription: plan, status, seat usage and what renews when.
 *
 * Seat usage drives a soft warning at 90% rather than a hard block — the API, not this panel,
 * is what actually enforces the ceiling; here we only nudge before it bites.
 */
export function CurrentPlanPanel({
  subscription,
  plan,
  onChangePlan,
  onCancel,
}: CurrentPlanPanelProps) {
  const currency = subscription.currency;
  const seatPct =
    subscription.seatsIncluded > 0
      ? (subscription.seatsUsed / subscription.seatsIncluded) * 100
      : 0;
  const nearSeatLimit = seatPct >= 90;

  return (
    <section className="rounded-[18px] border border-border bg-card p-5 shadow-soft">
      <h2 className="flex items-center gap-2 font-semibold">
        <BadgeCheck className="h-5 w-5 text-primary" aria-hidden />
        Current plan
      </h2>

      <div className="mt-4 flex flex-wrap items-center gap-x-3 gap-y-1">
        <span className="text-xl font-semibold">{plan.name}</span>
        <Badge variant={subscriptionStatusVariant[subscription.status]}>
          {subscriptionStatusLabel[subscription.status]}
        </Badge>
        <span className="text-sm text-muted-foreground">
          {subscription.cycle === "annual" ? "Billed annually" : "Billed monthly"}
        </span>
      </div>

      <div className="mt-5 space-y-1.5">
        <p className="text-sm text-muted-foreground">
          <span className="font-medium text-foreground tabular-nums">
            {subscription.seatsUsed}
          </span>{" "}
          of{" "}
          <span className="font-medium text-foreground tabular-nums">
            {subscription.seatsIncluded}
          </span>{" "}
          learner seats used
        </p>
        <Progress value={seatPct} label="Learner seats" />
        {nearSeatLimit && (
          <p className="flex items-center gap-1.5 text-xs text-warning">
            <AlertTriangle className="h-3.5 w-3.5 shrink-0" aria-hidden />
            Approaching your seat limit
          </p>
        )}
      </div>

      <dl className="mt-5 grid gap-3 sm:grid-cols-2">
        <div className="rounded-xl border border-border bg-muted/40 px-4 py-3">
          <dt className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <Receipt className="h-3.5 w-3.5" aria-hidden />
            Next invoice
          </dt>
          <dd className="mt-1 font-semibold tabular-nums">
            {formatMoney(subscription.nextInvoiceAmount, currency)}
          </dd>
        </div>
        <div className="rounded-xl border border-border bg-muted/40 px-4 py-3">
          <dt className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <CalendarClock className="h-3.5 w-3.5" aria-hidden />
            Renews on
          </dt>
          <dd className="mt-1 font-semibold">
            {formatDate(subscription.currentPeriodEndsOn)}
          </dd>
        </div>
      </dl>

      {subscription.status === "trialing" && subscription.trialEndsOn && (
        <p className="mt-4 flex items-center gap-1.5 text-sm text-muted-foreground">
          <Info className="h-3.5 w-3.5 shrink-0" aria-hidden />
          Trial ends {formatDate(subscription.trialEndsOn)}
        </p>
      )}

      {subscription.status === "canceled" && subscription.cancelsOn && (
        <p className="mt-4 flex items-center gap-1.5 text-sm text-warning">
          <AlertTriangle className="h-3.5 w-3.5 shrink-0" aria-hidden />
          Access ends {formatDate(subscription.cancelsOn)}
        </p>
      )}

      <div className="mt-5 flex flex-col gap-2 sm:flex-row">
        <Button variant="default" onClick={onChangePlan}>
          Change plan
        </Button>
        <Button variant="ghost" className="text-destructive" onClick={onCancel}>
          Cancel subscription
        </Button>
      </div>
    </section>
  );
}
