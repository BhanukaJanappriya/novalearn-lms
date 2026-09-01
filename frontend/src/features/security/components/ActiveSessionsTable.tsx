import { LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import { timeAgo } from "@/lib/format";
import type { SessionRow } from "../api/types";
import { timeUntil } from "../lib/security";

interface ActiveSessionsTableProps {
  sessions: SessionRow[];
  onRevoke: (session: SessionRow) => void;
  busySessionId: string | null;
}

/** Every active session across the platform, newest first. */
export function ActiveSessionsTable({ sessions, onRevoke, busySessionId }: ActiveSessionsTableProps) {
  if (sessions.length === 0) {
    return <p className="py-8 text-center text-sm text-muted-foreground">No active sessions match that search.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left">
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Account</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">IP address</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Signed in</th>
            <th scope="col" className="pb-2 font-medium text-muted-foreground">Expires</th>
            <th scope="col" className="pb-2 text-right font-medium text-muted-foreground">Actions</th>
          </tr>
        </thead>
        <tbody>
          {sessions.map((session) => (
            <tr key={session.id} className="border-b border-border last:border-0">
              <td className="py-3 pr-3">
                <span className="block font-medium">{session.userName}</span>
                <span className="block text-xs text-muted-foreground">{session.userEmail}</span>
              </td>
              <td className="py-3 pr-3 text-muted-foreground">{session.createdByIp ?? "Unknown"}</td>
              <td className="py-3 pr-3 text-muted-foreground">{timeAgo(session.createdAtUtc)}</td>
              <td className="py-3 pr-3 text-muted-foreground">{timeUntil(session.expiresAtUtc)}</td>
              <td className="py-3 text-right">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => onRevoke(session)}
                  isLoading={busySessionId === session.id}
                >
                  <LogOut className="h-3.5 w-3.5" />
                  Revoke
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
