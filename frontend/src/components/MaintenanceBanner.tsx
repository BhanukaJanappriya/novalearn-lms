import { AlertTriangle } from "lucide-react";
import { usePublicSettings } from "@/features/settings/api/queries";

/**
 * A fixed banner shown to everyone while the platform is in maintenance mode, not only to whoever
 * happens to hit a blocked request first. The public settings endpoint that feeds this is one of
 * the few routes the maintenance gate itself always leaves reachable, precisely so this can render
 * for the very visitors the gate is blocking.
 */
export function MaintenanceBanner() {
  const { data: platform } = usePublicSettings();

  if (!platform?.maintenanceModeEnabled) {
    return null;
  }

  return (
    <div
      role="status"
      className="flex items-center justify-center gap-2 border-b border-warning/20 bg-warning/10 px-4 py-2 text-center text-sm font-medium text-[hsl(var(--warning))]"
    >
      <AlertTriangle className="h-4 w-4 shrink-0" aria-hidden />
      {platform.maintenanceMessage ?? "The platform is undergoing maintenance."}
    </div>
  );
}
