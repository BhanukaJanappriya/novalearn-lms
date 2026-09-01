import { LockKeyholeOpen } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import type { LockedAccountRow } from "../api/types";
import { timeUntil } from "../lib/security";

interface LockedAccountsTableProps {
  accounts: LockedAccountRow[];
  onUnlock: (account: LockedAccountRow) => void;
  busyUserId: string | null;
}

/** Every account currently locked out by repeated failed sign-ins, soonest-to-unlock first. */
export function LockedAccountsTable({ accounts, onUnlock, busyUserId }: LockedAccountsTableProps) {
  if (accounts.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No accounts are locked out.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[640px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Account</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Failed attempts</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Unlocks</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Actions</th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((account) => (
            <tr key={account.userId} className="border-b border-border last:border-0">
              <td className="py-3 pr-3">
                <span className="block font-medium">{account.userName}</span>
                <span className="block text-xs text-muted-foreground">{account.userEmail}</span>
              </td>
              <td className="py-3 pr-3">
                <Badge variant="destructive">{account.accessFailedCount}</Badge>
              </td>
              <td className="py-3 pr-3 text-muted-foreground">{timeUntil(account.lockoutEnd)}</td>
              <td className="py-3 text-right">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => onUnlock(account)}
                  isLoading={busyUserId === account.userId}
                >
                  <LockKeyholeOpen className="h-3.5 w-3.5" />
                  Unlock
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
