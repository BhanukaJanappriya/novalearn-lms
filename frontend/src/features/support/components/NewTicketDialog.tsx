import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import { useCreateTicket } from "../api/queries";
import type { TicketCategory, TicketPriority } from "../api/types";
import { allCategories, allPriorities, categoryLabel } from "../lib/support";

interface NewTicketDialogProps {
  open: boolean;
  onClose: () => void;
}

/** Raises a new support ticket, then jumps straight to its thread. */
export function NewTicketDialog({ open, onClose }: NewTicketDialogProps) {
  const navigate = useNavigate();
  const create = useCreateTicket();

  const [subject, setSubject] = useState("");
  const [category, setCategory] = useState<TicketCategory>("Technical");
  const [priority, setPriority] = useState<TicketPriority>("Normal");
  const [message, setMessage] = useState("");

  const reset = () => {
    setSubject("");
    setCategory("Technical");
    setPriority("Normal");
    setMessage("");
    create.reset();
  };

  const close = () => {
    reset();
    onClose();
  };

  const submit = () => {
    create.mutate(
      { subject: subject.trim(), category, priority, message: message.trim() },
      {
        onSuccess: (ticket) => {
          reset();
          onClose();
          navigate(`/support/${ticket.id}`);
        },
      },
    );
  };

  const canSubmit = subject.trim().length > 0 && message.trim().length > 0;

  return (
    <Modal open={open} onClose={close} title="Raise a support ticket" className="max-w-lg">
      {create.isError && (
        <Alert variant="error" className="mb-4">
          {getApiErrorMessage(create.error)}
        </Alert>
      )}

      <div className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="ticket-subject">Subject</Label>
          <Input
            id="ticket-subject"
            value={subject}
            onChange={(e) => setSubject(e.target.value)}
            placeholder="Short summary of the issue"
            maxLength={200}
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <Label htmlFor="ticket-category">Category</Label>
            <select
              id="ticket-category"
              value={category}
              onChange={(e) => setCategory(e.target.value as TicketCategory)}
              className="h-10 w-full rounded-lg border border-border bg-background px-3 text-sm"
            >
              {allCategories.map((option) => (
                <option key={option} value={option}>
                  {categoryLabel[option]}
                </option>
              ))}
            </select>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="ticket-priority">Priority</Label>
            <select
              id="ticket-priority"
              value={priority}
              onChange={(e) => setPriority(e.target.value as TicketPriority)}
              className="h-10 w-full rounded-lg border border-border bg-background px-3 text-sm"
            >
              {allPriorities.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="ticket-message">What's going on?</Label>
          <Textarea
            id="ticket-message"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            placeholder="Describe the issue — what you expected, what happened instead."
            rows={5}
            maxLength={4000}
          />
        </div>
      </div>

      <div className="mt-6 flex justify-end gap-2">
        <Button variant="ghost" onClick={close} disabled={create.isPending}>
          Cancel
        </Button>
        <Button onClick={submit} isLoading={create.isPending} disabled={!canSubmit}>
          Submit ticket
        </Button>
      </div>
    </Modal>
  );
}
