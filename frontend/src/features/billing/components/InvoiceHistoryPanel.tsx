import { Download, ReceiptText } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { Invoice } from "../api/types";
import {
  formatDate,
  formatMoney,
  invoiceStatusLabel,
  invoiceStatusVariant,
} from "../lib/billing";

interface InvoiceHistoryPanelProps {
  invoices: Invoice[];
}

/**
 * Past invoices, newest-first as the provider hands them over.
 *
 * The PDF button is presentational: a live provider returns a hosted URL per invoice, which is
 * the only wiring this panel will need.
 */
export function InvoiceHistoryPanel({ invoices }: InvoiceHistoryPanelProps) {
  return (
    <section className="rounded-[18px] border border-border bg-card p-5 shadow-soft">
      <h2 className="flex items-center gap-2 font-semibold">
        <ReceiptText className="h-5 w-5 text-primary" aria-hidden />
        Invoice history
        <span className="font-normal text-muted-foreground">
          {invoices.length} {invoices.length === 1 ? "invoice" : "invoices"}
        </span>
      </h2>

      {invoices.length === 0 ? (
        <div className="flex flex-col items-center gap-2 py-10 text-center text-sm text-muted-foreground">
          <ReceiptText className="h-6 w-6" aria-hidden />
          No invoices yet.
        </div>
      ) : (
        <div className="mt-4 overflow-x-auto">
          <table className="w-full min-w-[640px] border-collapse text-sm">
            <thead>
              <tr className="border-b border-border text-left">
                <th scope="col" className="pb-2 font-medium text-muted-foreground">Invoice</th>
                <th scope="col" className="pb-2 font-medium text-muted-foreground">Date</th>
                <th scope="col" className="pb-2 font-medium text-muted-foreground">Description</th>
                <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Amount</th>
                <th scope="col" className="pb-2 font-medium text-muted-foreground">Status</th>
                <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Download</th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((invoice) => (
                <tr key={invoice.id} className="border-b border-border last:border-0">
                  <td className="py-3 pr-3 font-medium tabular-nums">{invoice.number}</td>
                  <td className="py-3 pr-3 text-muted-foreground">{formatDate(invoice.issuedOn)}</td>
                  <td className="py-3 pr-3">{invoice.description}</td>
                  <td className="py-3 pr-3 text-right tabular-nums">
                    {formatMoney(invoice.amount, invoice.currency)}
                  </td>
                  <td className="py-3 pr-3">
                    <Badge variant={invoiceStatusVariant[invoice.status]}>
                      {invoiceStatusLabel[invoice.status]}
                    </Badge>
                  </td>
                  <td className="py-3 text-right">
                    <Button variant="ghost" size="sm">
                      <Download className="h-3.5 w-3.5" aria-hidden />
                      PDF
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
