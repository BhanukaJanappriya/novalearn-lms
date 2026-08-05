import { useEffect, useRef } from "react";
import { useQueryClient } from "@tanstack/react-query";
import type { HubConnection } from "@microsoft/signalr";
import { tokenStore } from "@/services/apiClient";
import { notificationKeys } from "./queries";
import type { AppNotification } from "./types";

/** Client method names. Must match what SignalRNotificationPublisher sends. */
const NOTIFICATION_RECEIVED = "notificationReceived";
const UNREAD_COUNT_CHANGED = "unreadCountChanged";

const HUB_URL = "/hubs/notifications";

interface UseNotificationHubOptions {
  /** Connect only once there is a session to authenticate with. */
  enabled: boolean;
  /** Fired for each notification pushed while connected, for a toast. */
  onNotification?: (notification: AppNotification) => void;
}

/**
 * Keeps a live connection to the notification hub for as long as the user is signed in, and
 * folds pushed data straight into the query cache so the badge and feed update without a refetch.
 *
 * The SignalR client is imported dynamically: it is around 75 kB and nothing on first paint
 * needs it, so it must not sit in the eager bundle.
 *
 * The connection is best effort. If it never establishes, or drops permanently, the REST
 * endpoints still serve the feed on the next page load, so nothing is lost, it just stops
 * being instant.
 */
export function useNotificationHub({ enabled, onNotification }: UseNotificationHubOptions) {
  const queryClient = useQueryClient();

  // Kept in a ref so changing the callback does not tear down and rebuild the socket.
  const onNotificationRef = useRef(onNotification);
  onNotificationRef.current = onNotification;

  useEffect(() => {
    if (!enabled) {
      return;
    }

    let cancelled = false;
    let connection: HubConnection | null = null;

    void (async () => {
      const { HubConnectionBuilder, HubConnectionState, LogLevel } =
        await import("@microsoft/signalr");

      // The session may have ended while the chunk was downloading.
      if (cancelled) {
        return;
      }

      connection = new HubConnectionBuilder()
        .withUrl(HUB_URL, {
          // A WebSocket cannot carry an Authorization header, so the token goes on the query
          // string. The server only honours that on hub paths.
          accessTokenFactory: () => tokenStore.get() ?? "",
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build();

      connection.on(NOTIFICATION_RECEIVED, (notification: AppNotification) => {
        void queryClient.invalidateQueries({ queryKey: notificationKeys.all });
        onNotificationRef.current?.(notification);
      });

      connection.on(UNREAD_COUNT_CHANGED, (unreadCount: number) => {
        // Written straight into the cache so the badge moves without a round trip.
        queryClient.setQueryData(notificationKeys.unreadCount(), { unreadCount });
      });

      try {
        await connection.start();
      } catch {
        // A failed connection must not break the page; the feed still works over REST.
        return;
      }

      if (cancelled && connection.state !== HubConnectionState.Disconnected) {
        void connection.stop();
      }
    })();

    return () => {
      cancelled = true;
      void connection?.stop();
    };
  }, [enabled, queryClient]);
}
