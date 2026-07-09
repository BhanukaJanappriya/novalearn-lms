import { useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { Check, Eye, X } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { timeAgo } from "@/lib/format";
import type { ApprovalKind, PendingApproval } from "../api/types";

const kindLabel: Record<ApprovalKind, string> = {
  student: "Student",
  lecturer: "Lecturer",
  course: "Course",
  certificate: "Certificate",
};

/**
 * Interactive approval queue with optimistic resolve. In the live build the
 * approve/reject handlers become mutations; here they just drop the row locally.
 */
export function PendingApprovals({ items }: { items: PendingApproval[] }) {
  const [queue, setQueue] = useState(items);

  const resolve = (id: string) => setQueue((q) => q.filter((item) => item.id !== id));

  return (
    <div className="space-y-2">
      <AnimatePresence initial={false}>
        {queue.map((item) => (
          <motion.div
            key={item.id}
            layout
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, x: 24, height: 0, marginBottom: 0 }}
            transition={{ duration: 0.2 }}
            className="flex items-center gap-3 rounded-xl border border-border bg-background/40 p-3"
          >
            <Avatar name={item.name} color={item.color} size="md" />
            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-2">
                <p className="truncate text-sm font-medium">{item.name}</p>
                <Badge variant="neutral">{kindLabel[item.kind]}</Badge>
              </div>
              <p className="truncate text-xs text-muted-foreground">{item.subtitle}</p>
              <p className="mt-0.5 text-[11px] text-muted-foreground">
                {item.meta} · {timeAgo(item.submittedAt)}
              </p>
            </div>
            <div className="flex shrink-0 items-center gap-1">
              <Button variant="ghost" size="icon" className="h-8 w-8" aria-label="Preview" title="Preview">
                <Eye className="h-4 w-4" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-destructive hover:bg-destructive/10"
                aria-label="Reject"
                title="Reject"
                onClick={() => resolve(item.id)}
              >
                <X className="h-4 w-4" />
              </Button>
              <Button
                size="icon"
                className="h-8 w-8 bg-success text-white hover:bg-success/90"
                aria-label="Approve"
                title="Approve"
                onClick={() => resolve(item.id)}
              >
                <Check className="h-4 w-4" />
              </Button>
            </div>
          </motion.div>
        ))}
      </AnimatePresence>

      {queue.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-10 text-center">
          <Check className="mb-2 h-8 w-8 text-success" />
          <p className="text-sm font-medium">All caught up</p>
          <p className="text-xs text-muted-foreground">No pending approvals right now.</p>
        </div>
      )}
    </div>
  );
}
