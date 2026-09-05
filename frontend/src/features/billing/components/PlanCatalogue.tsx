/**
 * The full plan line-up with a monthly/annual switch. Owns only the billing-cycle toggle;
 * which plan is current and what a selection does are passed in by the parent.
 */
import { useState } from "react";
import { CreditCard } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { BillingCycle, Plan, PlanId } from "../api/types";
import { PlanCard } from "./PlanCard";

interface PlanCatalogueProps {
  plans: Plan[];
  currentPlanId: PlanId | null;
  onSelectPlan?: (planId: PlanId) => void;
}

const CYCLES: { value: BillingCycle; label: string }[] = [
  { value: "monthly", label: "Monthly" },
  { value: "annual", label: "Annual" },
];

export function PlanCatalogue({ plans, currentPlanId, onSelectPlan }: PlanCatalogueProps) {
  // Annual is the cheaper option, so it is the steered default.
  const [cycle, setCycle] = useState<BillingCycle>("annual");

  return (
    <section className="space-y-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="flex items-center gap-2 text-lg font-semibold">
            <CreditCard className="h-5 w-5 shrink-0 text-primary" aria-hidden />
            Plans
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Every plan includes unlimited courses, assessments and the full gradebook.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <div
            role="group"
            aria-label="Billing cycle"
            className="inline-flex rounded-full bg-muted p-1"
          >
            {CYCLES.map((option) => {
              const active = cycle === option.value;
              return (
                <button
                  key={option.value}
                  type="button"
                  aria-pressed={active}
                  onClick={() => setCycle(option.value)}
                  className={cn(
                    "rounded-full px-3 py-1 text-sm font-medium transition-colors",
                    active ? "bg-card text-foreground shadow-soft" : "text-muted-foreground",
                  )}
                >
                  {option.label}
                </button>
              );
            })}
          </div>
          <Badge variant="success">Save up to 20%</Badge>
        </div>
      </div>

      <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
        {plans.map((plan) => (
          <PlanCard
            key={plan.id}
            plan={plan}
            cycle={cycle}
            isCurrentPlan={plan.id === currentPlanId}
            onSelect={onSelectPlan}
          />
        ))}
      </div>
    </section>
  );
}
