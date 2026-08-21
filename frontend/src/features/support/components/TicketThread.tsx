import { Lock } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { timeAgo } from "@/lib/format";
import { avatarColor } from "@/features/users/lib/userVisuals";
import type { TicketMessage } from "../api/types";

interface TicketThreadProps {
  messages: TicketMessage[];
  submittedById: string;
}

/**
 * A ticket's messages in order. An internal note gets a tinted background and a lock icon rather
 * than a different layout — it is still part of the same conversation's timeline, just one the
 * submitter never receives.
 */
export function TicketThread({ messages, submittedById }: TicketThreadProps) {
  return (
    <div className="space-y-4">
      {messages.map((message) => {
        const isSubmitter = message.authorId === submittedById;

        return (
          <div
            key={message.id}
            className={`flex gap-3 rounded-[14px] border p-4 ${
              message.isInternalNote
                ? "border-warning/30 bg-warning/5"
                : "border-border bg-card"
            }`}
          >
            <Avatar
              name={message.authorName}
              src={message.authorAvatarUrl}
              color={avatarColor(message.authorId)}
              size="md"
            />
            <div className="min-w-0 flex-1">
              <div className="flex flex-wrap items-center gap-1.5">
                <span className="font-medium">{message.authorName}</span>
                {!isSubmitter && (
                  <span className="rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-medium text-primary">
                    Staff
                  </span>
                )}
                {message.isInternalNote && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-warning/15 px-2 py-0.5 text-[11px] font-medium text-warning">
                    <Lock className="h-3 w-3" aria-hidden />
                    Internal note
                  </span>
                )}
                <span className="text-xs text-muted-foreground">{timeAgo(message.createdAtUtc)}</span>
              </div>
              <p className="mt-1.5 whitespace-pre-wrap text-sm">{message.body}</p>
            </div>
          </div>
        );
      })}
    </div>
  );
}
