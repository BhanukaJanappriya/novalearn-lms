import { RotateCcw } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { timeAgo } from "@/lib/format";
import type { Transaction } from "../api/queries";
import { formatMoney, statusLabel, statusVariant } from "../lib/finance";

interface TransactionsTableProps {
  transactions: Transaction[];
  onRefund: (transaction: Transaction) => void;
}

/** The ledger: every payment, newest first. */
export function TransactionsTable({ transactions, onRefund }: TransactionsTableProps) {
  if (transactions.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No transactions match that search.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[760px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Course</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Student</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Amount</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Status</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">When</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Actions</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((transaction) => (
            <tr key={transaction.id} className="border-b border-border last:border-0">
              <td className="py-3 pr-3 font-medium">{transaction.courseTitle}</td>

              <td className="py-3 pr-3">
                <span className="block">{transaction.studentName}</span>
                <span className="block text-xs text-muted-foreground">{transaction.studentEmail}</span>
              </td>

              <td className="py-3 text-right tabular-nums">
                {formatMoney(transaction.amount, transaction.currency)}
                {transaction.refundedAmount ? (
                  <span className="block text-xs text-warning">
                    −{formatMoney(transaction.refundedAmount, transaction.currency)} refunded
                  </span>
                ) : null}
              </td>

              <td className="py-3 pr-3">
                <Badge variant={statusVariant[transaction.status]}>
                  {statusLabel[transaction.status]}
                </Badge>
                {transaction.failureReason && (
                  <span className="mt-0.5 block text-xs text-muted-foreground">
                    {transaction.failureReason}
                  </span>
                )}
              </td>

              <td className="py-3 pr-3 text-xs text-muted-foreground">
                {transaction.paidAtUtc
                  ? `Paid ${timeAgo(transaction.paidAtUtc)}`
                  : `Started ${timeAgo(transaction.createdAtUtc)}`}
              </td>

              <td className="py-3 text-right">
                {transaction.canRefund && (
                  <Button variant="ghost" size="sm" onClick={() => onRefund(transaction)}>
                    <RotateCcw className="h-3.5 w-3.5" />
                    Refund
                  </Button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
