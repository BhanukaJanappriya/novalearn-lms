import { useEffect, useState } from "react";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Modal } from "@/components/ui/modal";
import { getApiErrorMessage } from "@/lib/apiError";
import type { AdminUser } from "../api/types";
import { roleVariant, sortRoles } from "../lib/userVisuals";

interface UserRolesDialogProps {
  user: AdminUser | null;
  availableRoles: string[];
  /** Roles the signed-in admin is not allowed to grant or revoke. */
  lockedRoles: string[];
  open: boolean;
  onClose: () => void;
  onSave: (roles: string[]) => void;
  isSaving: boolean;
  error: unknown;
}

/**
 * Edits the exact role set for one account. The server is the authority on what is allowed;
 * this only hides options the caller certainly cannot use, so the rules are never duplicated
 * in a way that could drift.
 */
export function UserRolesDialog({
  user,
  availableRoles,
  lockedRoles,
  open,
  onClose,
  onSave,
  isSaving,
  error,
}: UserRolesDialogProps) {
  const [selected, setSelected] = useState<string[]>([]);

  // Re-seed whenever a different account is opened.
  useEffect(() => {
    setSelected(user ? sortRoles(user.roles) : []);
  }, [user]);

  if (!user) return null;

  const toggle = (role: string) =>
    setSelected((current) =>
      current.includes(role) ? current.filter((r) => r !== role) : [...current, role],
    );

  const unchanged =
    selected.length === user.roles.length && selected.every((r) => user.roles.includes(r));

  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Manage roles"
      description={`${user.fullName} · ${user.email}`}
    >
      <div className="space-y-4">
        {error ? <Alert variant="error">{getApiErrorMessage(error)}</Alert> : null}

        <ul className="space-y-1">
          {availableRoles.map((role) => {
            const locked = lockedRoles.includes(role);
            const checked = selected.includes(role);

            return (
              <li key={role}>
                <label
                  className={`flex items-center gap-3 rounded-xl border px-3 py-2.5 transition-colors ${
                    locked
                      ? "cursor-not-allowed border-transparent opacity-50"
                      : "cursor-pointer border-transparent hover:border-border hover:bg-muted/50"
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={checked}
                    disabled={locked || isSaving}
                    onChange={() => toggle(role)}
                    className="h-4 w-4 rounded border-border accent-primary"
                  />
                  <Badge variant={roleVariant(role)}>{role}</Badge>
                  {locked ? (
                    <span className="ml-auto text-xs text-muted-foreground">
                      Super administrators only
                    </span>
                  ) : null}
                </label>
              </li>
            );
          })}
        </ul>

        {selected.length === 0 ? (
          <p className="text-xs text-[hsl(var(--warning))]">
            Assign at least one role. To revoke access, deactivate the account instead.
          </p>
        ) : null}

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button
            onClick={() => onSave(selected)}
            isLoading={isSaving}
            disabled={selected.length === 0 || unchanged}
          >
            Save roles
          </Button>
        </div>
      </div>
    </Modal>
  );
}
