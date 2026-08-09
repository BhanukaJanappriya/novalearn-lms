import { useEffect, useState } from "react";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal } from "@/components/ui/modal";
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage } from "@/lib/apiError";
import type { AdminUser } from "@/features/users/api/types";
import type { Department, DepartmentInput } from "../api/types";

interface DepartmentFormDialogProps {
  /** Null creates a department; a department edits it. */
  department: Department | null;
  /** Accounts eligible to head a department, already filtered to teaching staff. */
  eligibleHeads: AdminUser[];
  open: boolean;
  onClose: () => void;
  onSubmit: (input: DepartmentInput) => void;
  isSaving: boolean;
  error: unknown;
}

const blank = { name: "", code: "", description: "", headId: "", isActive: true };

/** Handles both creating and editing, matching the other admin forms. */
export function DepartmentFormDialog({
  department,
  eligibleHeads,
  open,
  onClose,
  onSubmit,
  isSaving,
  error,
}: DepartmentFormDialogProps) {
  const [form, setForm] = useState(blank);

  useEffect(() => {
    setForm(
      department
        ? {
            name: department.name,
            code: department.code,
            description: department.description ?? "",
            headId: department.headId ?? "",
            isActive: department.isActive,
          }
        : blank,
    );
  }, [department, open]);

  const submit = () =>
    onSubmit({
      name: form.name.trim(),
      code: form.code.trim().toUpperCase(),
      description: form.description.trim() || null,
      headId: form.headId || null,
      isActive: form.isActive,
    });

  // Mirrors the server rule, so the form cannot submit something the API would reject.
  const codeIsValid = /^[A-Za-z0-9-]+$/.test(form.code.trim());
  const invalid = form.name.trim().length === 0 || form.code.trim().length === 0 || !codeIsValid;

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={department ? "Edit department" : "New department"}
      description={department ? department.name : "Departments group courses into faculties."}
    >
      <div className="space-y-4">
        {error ? <Alert variant="error">{getApiErrorMessage(error)}</Alert> : null}

        <div className="grid gap-4 sm:grid-cols-[1fr_140px]">
          <div className="space-y-1.5">
            <Label htmlFor="department-name">Name</Label>
            <Input
              id="department-name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              placeholder="Astronomy and Astrophysics"
              maxLength={150}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="department-code">Code</Label>
            <Input
              id="department-code"
              value={form.code}
              onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })}
              placeholder="ASTR"
              maxLength={20}
              aria-invalid={form.code.length > 0 && !codeIsValid}
            />
          </div>
        </div>

        {form.code.length > 0 && !codeIsValid && (
          <p className="text-xs text-destructive">
            A code may only contain letters, numbers and hyphens.
          </p>
        )}

        <div className="space-y-1.5">
          <Label htmlFor="department-description">Description</Label>
          <Textarea
            id="department-description"
            rows={3}
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            placeholder="What this department covers."
            maxLength={1000}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="department-head">Head of department</Label>
          <select
            id="department-head"
            value={form.headId}
            onChange={(e) => setForm({ ...form, headId: e.target.value })}
            className="h-10 w-full rounded-md border border-input bg-card px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <option value="">Not assigned</option>
            {eligibleHeads.map((user) => (
              <option key={user.id} value={user.id}>
                {user.fullName} · {user.email}
              </option>
            ))}
          </select>
          <p className="text-xs text-muted-foreground">
            Only lecturers and administrators can head a department.
          </p>
        </div>

        <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-border p-3">
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
            className="h-4 w-4 rounded border-border accent-primary"
          />
          <span className="text-sm">
            Active
            <span className="block text-xs text-muted-foreground">
              A retired department keeps its courses but is not offered for new ones.
            </span>
          </span>
        </label>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button onClick={submit} isLoading={isSaving} disabled={invalid}>
            {department ? "Save changes" : "Create department"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
