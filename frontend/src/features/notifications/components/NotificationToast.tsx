import { useEffect } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { X } from "lucide-react";
import { useNavigate } from "react-router-dom";
import type { AppNotification } from "../api/types";
import { notificationVisual } from "../lib/notifications";

interface NotificationToastProps {
  notification: AppNotification | null;
  onDismiss: () => void;
}

/** How long a toast stays before dismissing itself. */
const VISIBLE_MS = 6000;

/**
 * A single transient toast for a notification that arrived while the page was open. Deliberately
 * one at a time: the feed is the durable record, so a toast only needs to catch attention.
 */
export function NotificationToast({ notification, onDismiss }: NotificationToastProps) {
  const navigate = useNavigate();

  useEffect(() => {
    if (!notification) return;
    const id = window.setTimeout(onDismiss, VISIBLE_MS);
    return () => window.clearTimeout(id);
  }, [notification, onDismiss]);

  const visual = notification ? notificationVisual(notification.type) : null;
  const Icon = visual?.icon;

  const open = () => {
    if (notification?.link) {
      navigate(notification.link);
    }
    onDismiss();
  };

  return (
    <AnimatePresence>
      {notification && (
        <motion.div
          role="status"
          aria-live="polite"
          initial={{ opacity: 0, y: 16, scale: 0.97 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: 16, scale: 0.97 }}
          transition={{ type: "spring", stiffness: 320, damping: 26 }}
          className="fixed bottom-6 right-6 z-50 w-[min(22rem,calc(100vw-3rem))] rounded-[18px] border border-border bg-card p-4 shadow-soft"
        >
          <div className="flex items-start gap-3">
            {Icon && (
              <span className={`shrink-0 rounded-xl bg-muted p-2 ${visual!.accent}`}>
                <Icon className="h-4 w-4" aria-hidden />
              </span>
            )}
            <button
              type="button"
              onClick={open}
              className="min-w-0 flex-1 text-left"
              disabled={!notification.link}
            >
              <span className="block truncate text-sm font-semibold">{notification.title}</span>
              <span className="mt-0.5 block text-xs text-muted-foreground">{notification.message}</span>
            </button>
            <button
              type="button"
              onClick={onDismiss}
              aria-label="Dismiss notification"
              className="shrink-0 rounded-lg p-1 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
            >
              <X className="h-4 w-4" />
            </button>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
