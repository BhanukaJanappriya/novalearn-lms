import { BadgeCheck, BookOpen, GraduationCap, Lock, MailCheck, Shield, UserCheck, UserX } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { timeAgo } from "@/lib/format";
import type { AdminUser } from "../api/types";
import { avatarColor, roleVariant, sortRoles } from "../lib/userVisuals";

interface UsersTableProps {
  users: AdminUser[];
  /** Id of the signed-in admin, whose own row cannot be acted on. */
  currentUserId: string | undefined;
  /** Whether the viewer may act on super administrator accounts. */
  canManageSuperAdmins: boolean;
  onEditRoles: (user: AdminUser) => void;
  onToggleStatus: (user: AdminUser) => void;
  onVerifyEmail: (user: AdminUser) => void;
  busyUserId: string | null;
}

/**
 * The account directory. Horizontally scrollable rather than collapsing columns, so the
 * table never forces the page body to scroll sideways on a narrow screen.
 */
export function UsersTable({
  users,
  currentUserId,
  canManageSuperAdmins,
  onEditRoles,
  onToggleStatus,
  onVerifyEmail,
  busyUserId,
}: UsersTableProps) {
  return (
    <div className="overflow-x-auto rounded-[18px] border border-border">
      <table className="w-full min-w-[840px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border bg-muted/40 text-left">
            <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Account</th>
            <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Roles</th>
            <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">State</th>
            <th scope="col" className="px-4 py-3 font-medium text-muted-foreground">Activity</th>
            <th scope="col" className="px-4 py-3 text-right font-medium text-muted-foreground">Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => {
            const isSelf = user.id === currentUserId;
            const isSuperAdmin = user.roles.includes("SuperAdministrator");
            const locked = isSelf || (isSuperAdmin && !canManageSuperAdmins);
            const busy = busyUserId === user.id;

            return (
              <tr key={user.id} className="border-b border-border last:border-0 hover:bg-muted/30">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <Avatar
                      name={user.fullName}
                      src={user.avatarUrl}
                      color={avatarColor(user.email)}
                      size="md"
                    />
                    <span className="min-w-0">
                      <span className="flex items-center gap-1.5">
                        <span className="truncate font-medium">{user.fullName}</span>
                        {isSelf ? <Badge variant="outline">You</Badge> : null}
                      </span>
                      <span className="block truncate text-xs text-muted-foreground">{user.email}</span>
                    </span>
                  </div>
                </td>

                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-1">
                    {sortRoles(user.roles).map((role) => (
                      <Badge key={role} variant={roleVariant(role)}>
                        {role === "SuperAdministrator" ? (
                          <Shield className="h-3 w-3" aria-hidden />
                        ) : null}
                        {role}
                      </Badge>
                    ))}
                  </div>
                </td>

                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-1">
                    <Badge variant={user.isActive ? "success" : "neutral"}>
                      {user.isActive ? "Active" : "Deactivated"}
                    </Badge>
                    {!user.emailConfirmed && <Badge variant="warning">Unverified</Badge>}
                    {user.isLockedOut && (
                      <Badge variant="destructive">
                        <Lock className="h-3 w-3" aria-hidden />
                        Locked
                      </Badge>
                    )}
                  </div>
                </td>

                <td className="px-4 py-3">
                  <div className="flex flex-col gap-0.5 text-xs text-muted-foreground">
                    <span className="flex items-center gap-3">
                      {user.enrollmentCount > 0 && (
                        <span className="inline-flex items-center gap-1">
                          <GraduationCap className="h-3.5 w-3.5" aria-hidden />
                          {user.enrollmentCount}
                        </span>
                      )}
                      {user.coursesOwned > 0 && (
                        <span className="inline-flex items-center gap-1">
                          <BookOpen className="h-3.5 w-3.5" aria-hidden />
                          {user.coursesOwned}
                        </span>
                      )}
                    </span>
                    <span>
                      {user.lastLoginAtUtc
                        ? `Seen ${timeAgo(user.lastLoginAtUtc)}`
                        : "Never signed in"}
                    </span>
                  </div>
                </td>

                <td className="px-4 py-3">
                  <div className="flex items-center justify-end gap-1">
                    {!user.emailConfirmed && !locked && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => onVerifyEmail(user)}
                        disabled={busy}
                        title="Confirm this email and clear any lockout"
                      >
                        <MailCheck className="h-3.5 w-3.5" />
                        Verify
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onEditRoles(user)}
                      disabled={locked || busy}
                      title={locked ? "You cannot change this account" : "Manage roles"}
                    >
                      <BadgeCheck className="h-3.5 w-3.5" />
                      Roles
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onToggleStatus(user)}
                      disabled={locked || busy}
                      title={locked ? "You cannot change this account" : undefined}
                    >
                      {user.isActive ? (
                        <>
                          <UserX className="h-3.5 w-3.5" />
                          Deactivate
                        </>
                      ) : (
                        <>
                          <UserCheck className="h-3.5 w-3.5" />
                          Activate
                        </>
                      )}
                    </Button>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
