import { useCallback, useState, type ReactNode } from "react";
import { useAuth } from "@/context/AuthContext";
import { useNotificationHub } from "./api/useNotificationHub";
import { NotificationToast } from "./components/NotificationToast";
import type { AppNotification } from "./api/types";

/**
 * Holds the single live connection for the whole app and surfaces arrivals as a toast.
 *
 * Mounted once above the router so one socket serves every page: putting the hook in the bell
 * would open a connection per header, and the learner and admin chromes each have one.
 */
export function NotificationsProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const [toast, setToast] = useState<AppNotification | null>(null);

  const dismiss = useCallback(() => setToast(null), []);

  useNotificationHub({
    enabled: isAuthenticated,
    onNotification: setToast,
  });

  return (
    <>
      {children}
      <NotificationToast notification={toast} onDismiss={dismiss} />
    </>
  );
}
