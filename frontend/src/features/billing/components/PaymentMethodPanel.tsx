import { AlertTriangle, CreditCard, Mail, User } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { PaymentMethod } from "../api/types";
import { cardBrandLabel } from "../lib/billing";

interface PaymentMethodPanelProps {
  method: PaymentMethod;
  onUpdate?: () => void;
}

// The billing area has no live clock, so "expires soon" is measured against a fixed reference
// date. Swapping the mock for a real provider is when this becomes `new Date()`.
const TODAY = new Date(2026, 8, 5);
const EXPIRY_WARNING_MONTHS = 3;

/**
 * The card on file, shown as a card: brand, last four, expiry and who it belongs to.
 *
 * Flags a card that lapses within the next three months so a failed renewal is caught before it
 * happens, not after the invoice bounces.
 */
export function PaymentMethodPanel({ method, onUpdate }: PaymentMethodPanelProps) {
  // A card is valid through the end of its exp month, i.e. invalid from the 1st of the next one.
  const expiresAt = new Date(method.expYear, method.expMonth, 1);
  const threshold = new Date(
    TODAY.getFullYear(),
    TODAY.getMonth() + EXPIRY_WARNING_MONTHS,
    TODAY.getDate(),
  );
  const expiresSoon = expiresAt <= threshold;

  return (
    <section className="rounded-[18px] border border-border bg-card p-5 shadow-soft">
      <h2 className="flex items-center gap-2 font-semibold">
        <CreditCard className="h-5 w-5 text-primary" aria-hidden />
        Payment method
      </h2>

      <div className="mt-4 block rounded-xl border border-border bg-muted/40 px-4 py-3">
        <p className="flex items-center gap-2 font-medium">
          <CreditCard className="h-4 w-4 text-muted-foreground" aria-hidden />
          {cardBrandLabel[method.brand]} ending {method.last4}
        </p>
        <p className="mt-1 text-sm text-muted-foreground">
          Expires {String(method.expMonth).padStart(2, "0")}/{method.expYear}
        </p>
      </div>

      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        <div>
          <dt className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <User className="h-3.5 w-3.5" aria-hidden />
            Cardholder
          </dt>
          <dd className="mt-1 text-sm font-medium">{method.holderName}</dd>
        </div>
        <div>
          <dt className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <Mail className="h-3.5 w-3.5" aria-hidden />
            Billing email
          </dt>
          <dd className="mt-1 text-sm font-medium">{method.billingEmail}</dd>
        </div>
      </dl>

      {expiresSoon && (
        <p className="mt-4 flex items-center gap-1.5 text-xs text-warning">
          <AlertTriangle className="h-3.5 w-3.5 shrink-0" aria-hidden />
          This card expires soon
        </p>
      )}

      <div className="mt-5">
        <Button variant="outline" size="sm" onClick={onUpdate}>
          Update payment method
        </Button>
      </div>
    </section>
  );
}
