import { useState } from "react";
import { Lock, Send } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";

interface ReplyBoxProps {
  onSubmit: (body: string, isInternalNote: boolean) => void;
  isPending: boolean;
  error: unknown;
  /** Staff get the option to keep a reply internal; a submitter never does. */
  allowInternalNote?: boolean;
  disabled?: boolean;
  disabledReason?: string;
}

/** The compose box at the bottom of a ticket thread. */
export function ReplyBox({
  onSubmit,
  isPending,
  error,
  allowInternalNote = false,
  disabled = false,
  disabledReason,
}: ReplyBoxProps) {
  const [body, setBody] = useState("");
  const [isInternalNote, setIsInternalNote] = useState(false);

  const submit = () => {
    if (!body.trim()) return;
    onSubmit(body.trim(), isInternalNote);
    setBody("");
    setIsInternalNote(false);
  };

  if (disabled) {
    return (
      <p className="rounded-[14px] border border-dashed border-border px-4 py-3 text-center text-sm text-muted-foreground">
        {disabledReason ?? "This ticket is closed."}
      </p>
    );
  }

  return (
    <div className="space-y-2">
      {error !== null && error !== undefined && (
        <Alert variant="error">{getApiErrorMessage(error)}</Alert>
      )}
      <Textarea
        value={body}
        onChange={(e) => setBody(e.target.value)}
        placeholder={isInternalNote ? "Note visible to staff only…" : "Write a reply…"}
        rows={3}
        maxLength={4000}
      />
      <div className="flex items-center justify-between gap-2">
        {allowInternalNote ? (
          <label className="flex cursor-pointer items-center gap-1.5 text-xs text-muted-foreground">
            <input
              type="checkbox"
              checked={isInternalNote}
              onChange={(e) => setIsInternalNote(e.target.checked)}
              className="h-3.5 w-3.5 rounded border-input"
            />
            <Lock className="h-3 w-3" aria-hidden />
            Internal note (not visible to the submitter)
          </label>
        ) : (
          <span />
        )}
        <Button size="sm" onClick={submit} isLoading={isPending} disabled={!body.trim()}>
          <Send className="h-3.5 w-3.5" />
          {isInternalNote ? "Add note" : "Send reply"}
        </Button>
      </div>
    </div>
  );
}
