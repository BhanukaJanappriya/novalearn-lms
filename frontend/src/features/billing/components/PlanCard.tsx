/**
 * One plan in the pricing catalogue: name, price for the chosen billing cycle, what it includes
 * and a single call to action. Purely presentational — choosing a plan is the parent's concern.
 */
import { Building2, Check, Minus, Rocket, Sparkles, TrendingUp, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { BillingCycle, Plan, PlanId } from "../api/types";
import { annualSaving, formatMoney, monthlyPrice } from "../lib/billing";

// Plan identity is fixed, so the icon is a static lookup rather than another prop to thread.
const PLAN_ICON: Record<PlanId, typeof Rocket> = {
  starter: Rocket,
  growth: TrendingUp,
  scale: Building2,
};

interface PlanCardProps {
  plan: Plan;
  cycle: BillingCycle;
  isCurrentPlan: boolean;
  onSelect?: (planId: PlanId) => void;
}

export function PlanCard({ plan, cycle, isCurrentPlan, onSelect }: PlanCardProps) {
  const PlanIcon = PLAN_ICON[plan.id];
  const price = formatMoney(monthlyPrice(plan, cycle), plan.currency);

  // CTA copy and styling depend on where this plan sits relative to the one already active.
  const cta = isCurrentPlan
    ? { label: "Current plan", variant: "outline" as const, disabled: true }
    : plan.id === "scale"
      ? { label: "Contact sales", variant: "outline" as const, disabled: false }
      : { label: `Choose ${plan.name}`, variant: "default" as const, disabled: false };

  return (
    <div
      className={cn(
        "relative flex h-full flex-col rounded-[18px] border bg-card p-6 shadow-soft",
        plan.recommended ? "border-primary ring-1 ring-primary" : "border-border",
      )}
    >
      {plan.recommended && (
        <Badge variant="default" className="absolute -top-3 left-6 border border-primary bg-card">
          <Sparkles className="h-3 w-3" aria-hidden />
          Most popular
        </Badge>
      )}

      <div className="flex items-center gap-2">
        <PlanIcon className="h-5 w-5 shrink-0 text-primary" aria-hidden />
        <h3 className="text-base font-semibold text-foreground">{plan.name}</h3>
      </div>
      <p className="mt-1 text-sm text-muted-foreground">{plan.tagline}</p>

      <div className="mt-4">
        <p className="flex items-baseline gap-1">
          <span className="text-3xl font-semibold tracking-tight text-foreground">{price}</span>
          <span className="text-sm text-muted-foreground">/ month</span>
        </p>
        {cycle === "annual" ? (
          <p className="mt-1 text-xs text-success">
            billed annually — save {formatMoney(annualSaving(plan), plan.currency)} a year
          </p>
        ) : (
          // Keep the line rendered so a card is the same height in either cycle.
          <p className="mt-1 text-xs text-muted-foreground">billed monthly</p>
        )}
      </div>

      <p className="mt-3 flex items-center gap-2 text-sm text-muted-foreground">
        <Users className="h-4 w-4 shrink-0" aria-hidden />
        {plan.seatSummary}
      </p>

      <hr className="my-4 border-border" />

      <ul className="space-y-2.5 text-sm">
        {plan.benefits.map((benefit) => (
          <li key={benefit.label} className="flex gap-2">
            {benefit.included ? (
              <Check className="mt-0.5 h-4 w-4 shrink-0 text-success" aria-hidden />
            ) : (
              <Minus className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" aria-hidden />
            )}
            <span>
              <span
                className={cn(
                  benefit.included ? "text-foreground" : "text-muted-foreground line-through",
                )}
              >
                {benefit.label}
              </span>
              {benefit.detail && (
                <span className="block text-xs text-muted-foreground">{benefit.detail}</span>
              )}
            </span>
          </li>
        ))}
      </ul>

      {/* mt-auto pins the CTA to the bottom so cards in a row line their buttons up. */}
      <Button
        className="mt-auto w-full"
        variant={cta.variant}
        disabled={cta.disabled}
        onClick={() => onSelect?.(plan.id)}
      >
        {cta.label}
      </Button>
    </div>
  );
}
