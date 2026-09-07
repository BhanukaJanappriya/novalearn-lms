import { Check, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { passwordRules } from "./schemas";

/**
 * Live checklist under the password field. Every rule here is also enforced by the zod schema and
 * again by the backend, so this is guidance, not the gate. Hidden until the user starts typing to
 * keep the initial form quiet.
 */
export function PasswordRequirements({ value }: { value: string }) {
  if (!value) return null;

  return (
    <ul className="mt-2 grid gap-1" aria-label="Password requirements">
      {passwordRules.map((rule) => {
        const met = rule.test(value);
        return (
          <li
            key={rule.label}
            className={cn(
              "flex items-center gap-1.5 text-xs",
              met ? "text-success" : "text-muted-foreground",
            )}
          >
            {met ? (
              <Check className="h-3.5 w-3.5 shrink-0" aria-hidden />
            ) : (
              <X className="h-3.5 w-3.5 shrink-0" aria-hidden />
            )}
            <span>{rule.label}</span>
          </li>
        );
      })}
    </ul>
  );
}
