import { useState } from "react";
import {
  KeyRound,
  Lock,
  ShieldAlert,
  ShieldCheck,
  Search,
  TriangleAlert,
} from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { PaginationControls } from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { getApiErrorMessage } from "@/lib/apiError";
import { ChartCard } from "@/features/admin/components/charts/ChartCard";
import {
  useActiveSessions,
  useLockedAccounts,
  useRevokeSession,
  useSecurityOverview,
  useUnlockAccount,
} from "../api/queries";
import type { LockedAccountRow, SessionRow } from "../api/types";
import { ActiveSessionsTable } from "../components/ActiveSessionsTable";
import { LockedAccountsTable } from "../components/LockedAccountsTable";

const PAGE_SIZE = 10;

/**
 * The security center: who is currently signed in, who is locked out, and the levers to act on
 * both. Deliberately not the dashboard's decorative "posture score" widget — every figure and
 * every row here comes straight off the tables that actually gate sign-in, and every action here
 * does something real: forcing a session to end, or clearing a lockout right now.
 */
export function SecurityPage() {
  const { data: overview, isLoading: overviewLoading, isError: overviewError, error: overviewErr } =
    useSecurityOverview();

  const [sessionSearch, setSessionSearch] = useState("");
  const debouncedSessionSearch = useDebouncedValue(sessionSearch, 300);
  const [sessionPage, setSessionPage] = useState(1);
  const sessions = useActiveSessions({
    search: debouncedSessionSearch,
    page: sessionPage,
    pageSize: PAGE_SIZE,
  });

  const [lockedSearch, setLockedSearch] = useState("");
  const debouncedLockedSearch = useDebouncedValue(lockedSearch, 300);
  const [lockedPage, setLockedPage] = useState(1);
  const lockedAccounts = useLockedAccounts({
    search: debouncedLockedSearch,
    page: lockedPage,
    pageSize: PAGE_SIZE,
  });

  const revokeSession = useRevokeSession();
  const unlockAccount = useUnlockAccount();

  const [pendingRevoke, setPendingRevoke] = useState<SessionRow | null>(null);
  const [pendingUnlock, setPendingUnlock] = useState<LockedAccountRow | null>(null);

  const confirmRevoke = () => {
    if (!pendingRevoke) return;
    revokeSession.mutate(pendingRevoke.id, { onSuccess: () => setPendingRevoke(null) });
  };

  const confirmUnlock = () => {
    if (!pendingUnlock) return;
    unlockAccount.mutate(pendingUnlock.userId, { onSuccess: () => setPendingUnlock(null) });
  };

  const cards = overview && [
    { label: "Active sessions", value: overview.activeSessions, icon: KeyRound },
    { label: "Locked accounts", value: overview.lockedOutAccounts, icon: Lock },
    { label: "Failed logins", value: overview.failedLoginAttempts, icon: ShieldAlert },
    { label: "2FA adoption", value: `${overview.twoFactorAdoptionPct}%`, icon: ShieldCheck },
  ];

  return (
    <PageTransition>
      <div className="space-y-6">
        <header>
          <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <ShieldCheck className="h-6 w-6 text-primary" aria-hidden />
            Security
          </h1>
          <p className="mt-1 text-muted-foreground">
            Who is currently signed in, who is locked out, and the tools to act on both.
          </p>
        </header>

        {overviewError && (
          <Alert variant="error">{getApiErrorMessage(overviewErr, "We could not load security data.")}</Alert>
        )}

        {overviewLoading ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton key={index} className="h-24 rounded-[18px]" />
            ))}
          </div>
        ) : (
          cards && (
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {cards.map((card) => (
                <div key={card.label} className="rounded-[18px] border border-border bg-card p-4 shadow-soft">
                  <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
                    <card.icon className="h-3.5 w-3.5" aria-hidden />
                    {card.label}
                  </p>
                  <p className="mt-1 text-2xl font-semibold tabular-nums">{card.value}</p>
                </div>
              ))}
            </div>
          )
        )}

        <ChartCard title="Active sessions" subtitle="Every session currently signed in, newest first">
          <div className="mb-4 flex flex-wrap items-center gap-2">
            <div className="relative min-w-[220px] flex-1 sm:flex-initial">
              <Search
                className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
                aria-hidden
              />
              <Input
                value={sessionSearch}
                onChange={(e) => {
                  setSessionSearch(e.target.value);
                  setSessionPage(1);
                }}
                placeholder="Search by name or email"
                aria-label="Search sessions"
                className="h-9 pl-9"
              />
            </div>
          </div>

          {sessions.isError && (
            <Alert variant="error">{getApiErrorMessage(sessions.error, "We could not load sessions.")}</Alert>
          )}

          {sessions.isLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 5 }).map((_, index) => (
                <Skeleton key={index} className="h-14 rounded-lg" />
              ))}
            </div>
          ) : sessions.data ? (
            <>
              <ActiveSessionsTable
                sessions={sessions.data.items}
                onRevoke={setPendingRevoke}
                busySessionId={revokeSession.isPending ? (revokeSession.variables ?? null) : null}
              />
              <div className="mt-4">
                <PaginationControls
                  page={sessions.data.page}
                  totalPages={sessions.data.totalPages}
                  totalCount={sessions.data.totalCount}
                  onPageChange={setSessionPage}
                  noun="session"
                />
              </div>
            </>
          ) : null}
        </ChartCard>

        <ChartCard title="Locked accounts" subtitle="Accounts locked out by repeated failed sign-ins">
          <div className="mb-4 flex flex-wrap items-center gap-2">
            <div className="relative min-w-[220px] flex-1 sm:flex-initial">
              <Search
                className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
                aria-hidden
              />
              <Input
                value={lockedSearch}
                onChange={(e) => {
                  setLockedSearch(e.target.value);
                  setLockedPage(1);
                }}
                placeholder="Search by name or email"
                aria-label="Search locked accounts"
                className="h-9 pl-9"
              />
            </div>
          </div>

          {lockedAccounts.isError && (
            <Alert variant="error">
              {getApiErrorMessage(lockedAccounts.error, "We could not load locked accounts.")}
            </Alert>
          )}

          {lockedAccounts.isLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 3 }).map((_, index) => (
                <Skeleton key={index} className="h-14 rounded-lg" />
              ))}
            </div>
          ) : lockedAccounts.data ? (
            <>
              <LockedAccountsTable
                accounts={lockedAccounts.data.items}
                onUnlock={setPendingUnlock}
                busyUserId={unlockAccount.isPending ? (unlockAccount.variables ?? null) : null}
              />
              <div className="mt-4">
                <PaginationControls
                  page={lockedAccounts.data.page}
                  totalPages={lockedAccounts.data.totalPages}
                  totalCount={lockedAccounts.data.totalCount}
                  onPageChange={setLockedPage}
                  noun="account"
                />
              </div>
            </>
          ) : null}
        </ChartCard>
      </div>

      <Modal
        open={pendingRevoke !== null}
        onClose={() => setPendingRevoke(null)}
        title="Revoke session"
        description={pendingRevoke?.userEmail}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
            <p className="text-muted-foreground">
              {pendingRevoke?.userName} will be signed out of this session immediately and will need to sign in
              again to continue.
            </p>
          </div>
          {revokeSession.isError && (
            <Alert variant="error">{getApiErrorMessage(revokeSession.error)}</Alert>
          )}
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setPendingRevoke(null)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={confirmRevoke} isLoading={revokeSession.isPending}>
              Revoke session
            </Button>
          </div>
        </div>
      </Modal>

      <Modal
        open={pendingUnlock !== null}
        onClose={() => setPendingUnlock(null)}
        title="Unlock account"
        description={pendingUnlock?.userEmail}
      >
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-xl bg-muted p-3 text-sm">
            <TriangleAlert className="mt-0.5 h-4 w-4 shrink-0 text-[hsl(var(--warning))]" aria-hidden />
            <p className="text-muted-foreground">
              {pendingUnlock?.userName} will be able to sign in again immediately. The failed-attempt count resets
              to zero.
            </p>
          </div>
          {unlockAccount.isError && (
            <Alert variant="error">{getApiErrorMessage(unlockAccount.error)}</Alert>
          )}
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setPendingUnlock(null)}>
              Cancel
            </Button>
            <Button onClick={confirmUnlock} isLoading={unlockAccount.isPending}>
              Unlock account
            </Button>
          </div>
        </div>
      </Modal>
    </PageTransition>
  );
}
