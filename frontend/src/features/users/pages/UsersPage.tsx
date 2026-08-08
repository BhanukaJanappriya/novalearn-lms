import { useEffect, useMemo, useState } from "react";
import { TriangleAlert, Users } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Modal } from "@/components/ui/modal";
import { PaginationControls } from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { useAuth } from "@/context/AuthContext";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import {
  useAssignableRoles,
  useSetUserRoles,
  useSetUserStatus,
  useUsers,
  useVerifyUserEmail,
} from "../api/queries";
import type { AdminUser, UserFilters as Filters } from "../api/types";
import { UserFilters, type StatusFilter } from "../components/UserFilters";
import { UserRolesDialog } from "../components/UserRolesDialog";
import { UsersTable } from "../components/UsersTable";

const PAGE_SIZE = 15;

/** Translates the status chip into the two independent server filters it stands for. */
function statusToFilters(status: StatusFilter): Pick<Filters, "isActive" | "emailConfirmed"> {
  switch (status) {
    case "active":
      return { isActive: true };
    case "inactive":
      return { isActive: false };
    case "unverified":
      return { emailConfirmed: false };
    default:
      return {};
  }
}

export function UsersPage() {
  const { user: currentUser } = useAuth();
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const [status, setStatus] = useState<StatusFilter>("all");
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<AdminUser | null>(null);
  const [pendingStatus, setPendingStatus] = useState<AdminUser | null>(null);

  const debouncedSearch = useDebouncedValue(search, 300);

  // Any filter change invalidates the current page number.
  useEffect(() => setPage(1), [debouncedSearch, role, status]);

  const filters: Filters = useMemo(
    () => ({ search: debouncedSearch, role, ...statusToFilters(status), page, pageSize: PAGE_SIZE }),
    [debouncedSearch, role, status, page],
  );

  const { data, isLoading, isError, error } = useUsers(filters);
  const { data: roles } = useAssignableRoles();
  const setRoles = useSetUserRoles();
  const setStatusMutation = useSetUserStatus();
  const verifyEmail = useVerifyUserEmail();

  const isSuperAdmin = currentUser?.roles.includes("SuperAdministrator") ?? false;
  const lockedRoles = isSuperAdmin ? [] : ["SuperAdministrator"];
  const busyUserId =
    setRoles.isPending || setStatusMutation.isPending || verifyEmail.isPending
      ? (setRoles.variables?.userId ?? setStatusMutation.variables?.userId ?? verifyEmail.variables ?? null)
      : null;

  const confirmStatusChange = () => {
    if (!pendingStatus) return;
    setStatusMutation.mutate(
      { userId: pendingStatus.id, isActive: !pendingStatus.isActive },
      { onSuccess: () => setPendingStatus(null) },
    );
  };

  const saveRoles = (next: string[]) => {
    if (!editing) return;
    setRoles.mutate({ userId: editing.id, roles: next }, { onSuccess: () => setEditing(null) });
  };

  const actionError = setStatusMutation.error ?? verifyEmail.error;

  return (
    <PageTransition>
      <div className="space-y-6">
      <header>
        <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
          <Users className="h-6 w-6 text-primary" aria-hidden />
          Users
        </h1>
        <p className="mt-1 text-muted-foreground">
          Manage accounts, roles and access across the platform.
        </p>
      </header>

      {isError && <Alert variant="error">{getApiErrorMessage(error, "We could not load accounts.")}</Alert>}
      {actionError ? <Alert variant="error">{getApiErrorMessage(actionError)}</Alert> : null}

      <UserFilters
        search={search}
        onSearchChange={setSearch}
        role={role}
        onRoleChange={setRole}
        status={status}
        onStatusChange={setStatus}
        roles={roles ?? []}
        totalCount={data?.totalCount ?? 0}
      />

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 6 }).map((_, index) => (
            <Skeleton key={index} className="h-16 rounded-xl" />
          ))}
        </div>
      ) : data && data.items.length === 0 ? (
        <div className="rounded-[18px] border border-dashed border-border py-16 text-center">
          <p className="font-medium">No accounts match these filters.</p>
          <p className="mt-1 text-sm text-muted-foreground">Try clearing the search or role filter.</p>
        </div>
      ) : data ? (
        <>
          <UsersTable
            users={data.items}
            currentUserId={currentUser?.id}
            canManageSuperAdmins={isSuperAdmin}
            onEditRoles={setEditing}
            onToggleStatus={setPendingStatus}
            onVerifyEmail={(u) => verifyEmail.mutate(u.id)}
            busyUserId={busyUserId}
          />
          <PaginationControls
            page={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            onPageChange={setPage}
            noun="account"
          />
        </>
      ) : null}

      <UserRolesDialog
        user={editing}
        availableRoles={roles ?? []}
        lockedRoles={lockedRoles}
        open={editing !== null}
        onClose={() => setEditing(null)}
        onSave={saveRoles}
        isSaving={setRoles.isPending}
        error={setRoles.error}
      />

      <Modal
        open={pendingStatus !== null}
        onClose={() => setPendingStatus(null)}
        title={pendingStatus?.isActive ? "Deactivate account" : "Activate account"}
        description={pendingStatus?.email}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
            <p className="text-muted-foreground">
              {pendingStatus?.isActive
                ? "They will be signed out and cannot sign in again until reactivated. Their enrolments and courses are kept."
                : "They will be able to sign in again immediately."}
            </p>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setPendingStatus(null)}>
              Cancel
            </Button>
            <Button
              variant={pendingStatus?.isActive ? "destructive" : "default"}
              onClick={confirmStatusChange}
              isLoading={setStatusMutation.isPending}
            >
              {pendingStatus?.isActive ? "Deactivate" : "Activate"}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
    </PageTransition>
  );
}
