import { useEffect, useState } from "react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { getApiErrorMessage } from "@/lib/apiError";
import type { Transaction } from "../api/queries";
import { useRefundPayment } from "../api/queries";
import { formatMoney } from "../lib/finance";

interface RefundDialogProps {
  transaction: Transaction | null;
  onClose: () => void;
}

/** Refunds a transaction, in whole or in part. Talks to Stripe directly — this is real money. */
export function RefundDialog({ transaction, onClose }: RefundDialogProps) {
  const refund = useRefundPayment();
  const [amountText, setAmountText] = useState("");

  useEffect(() => {
    if (transaction) {
      setAmountText(transaction.refundableAmount.toFixed(2));
      refund.reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [transaction?.id]);

  if (!transaction) return null;

  const amount = Number(amountText);
  const validAmount =
    Number.isFinite(amount) && amount > 0 && amount <= transaction.refundableAmount + 0.001;

  const close = () => {
    refund.reset();
    onClose();
  };

  const submit = () => {
    const full = Math.abs(amount - transaction.refundableAmount) < 0.005;

    refund.mutate(
      { paymentId: transaction.id, amount: full ? undefined : amount },
      { onSuccess: close },
    );
  };

  return (
    <Modal open onClose={close} title="Refund this payment" className="max-w-md">
      <div className="space-y-1 text-sm">
        <p className="font-medium">{transaction.courseTitle}</p>
        <p className="text-muted-foreground">
          {transaction.studentName} · paid {formatMoney(transaction.amount, transaction.currency)}
          {transaction.refundedAmount ? (
            <> · already refunded {formatMoney(transaction.refundedAmount, transaction.currency)}</>
          ) : null}
        </p>
      </div>

      {refund.isError && (
        <Alert variant="error" className="mt-4">
          {getApiErrorMessage(refund.error)}
        </Alert>
      )}

      <div className="mt-4 space-y-1.5">
        <Label htmlFor="refund-amount">Amount to refund</Label>
        <Input
          id="refund-amount"
          type="number"
          step="0.01"
          min={0.01}
          max={transaction.refundableAmount}
          value={amountText}
          onChange={(event) => setAmountText(event.target.value)}
          aria-invalid={!validAmount}
        />
        <p className="text-xs text-muted-foreground">
          Up to {formatMoney(transaction.refundableAmount, transaction.currency)} left to refund.
        </p>
        {!validAmount && amountText !== "" && (
          <p className="text-xs text-destructive">Enter an amount up to what's left to refund.</p>
        )}
      </div>

      <div className="mt-6 flex justify-end gap-2">
        <Button variant="ghost" onClick={close} disabled={refund.isPending}>
          Cancel
        </Button>
        <Button
          variant="destructive"
          onClick={submit}
          isLoading={refund.isPending}
          disabled={!validAmount}
        >
          Refund {amountText && validAmount ? formatMoney(amount, transaction.currency) : ""}
        </Button>
      </div>
    </Modal>
  );
}
