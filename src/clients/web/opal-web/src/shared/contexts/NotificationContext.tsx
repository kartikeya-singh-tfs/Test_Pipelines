import React, {
  createContext,
  useContext,
  useState,
  ReactNode,
  useCallback,
  useEffect,
  useRef,
} from "react";
import { useGlobalSSE } from "./GlobalSSEContext";

export interface Notification {
  id: string;
  message: string;
  type?: "info" | "success" | "warning" | "error";
  timestamp: number;
  read?: boolean;
}

interface NotificationContextType {
  notifications: Notification[];
  unreadCount: number;
  markAllAsRead: () => void;
  addNotification: (
    notification: Omit<Notification, "id" | "timestamp" | "read">,
  ) => void;
  clearNotifications: () => void;
  // New method to register a sequence when submitted
  registerSequence: (sequenceId: string, sequenceName: string) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(
  undefined,
);

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context)
    throw new Error("useNotification must be used within NotificationProvider");
  return context;
};

export const NotificationProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const { subscribeToSequenceUpdates } = useGlobalSSE();
  const lastNotifiedSequenceId = useRef<string | null>(null);
  // Simple map to store sequence names as we see them
  const sequenceNames = useRef<Map<string, string>>(new Map());

  // Count unread notifications
  const unreadCount = notifications.filter((n) => !n.read).length;

  // Mark all as read
  const markAllAsRead = useCallback(() => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  }, []);

  // Prevent only true duplicate notifications (identical message and type within 2 seconds)
  const addNotification = useCallback(
    (notification: Omit<Notification, "id" | "timestamp" | "read">) => {
      setNotifications((prev) => {
        const now = Date.now();
        // Find the most recent notification with the same message and type
        const lastSimilar = prev.find(
          (n) =>
            n.message === notification.message && n.type === notification.type,
        );
        if (lastSimilar && now - lastSimilar.timestamp < 2000) {
          return prev;
        }
        return [
          {
            ...notification,
            id: Math.random().toString(36).substr(2, 9),
            timestamp: now,
            read: false,
          },
          ...prev,
        ];
      });
    },
    [],
  );

  const clearNotifications = useCallback(() => {
    setNotifications([]);
  }, []);

  // Register a sequence when it's submitted - stores name and shows submission notification
  const registerSequence = useCallback(
    (sequenceId: string, sequenceName: string) => {
      sequenceNames.current.set(sequenceId, sequenceName);

      // Show submission notification
      addNotification({
        message: `Acquisition ${sequenceName} submitted.`,
        type: "success",
      });
    },
    [addNotification],
  );

  // Simple effect: listen for sequence completion events only
  useEffect(() => {
    const unsubscribe = subscribeToSequenceUpdates((message) => {
      const { SequenceId, SequenceState } = message.Data;

      // Handle sequence completion
      if (SequenceState === "SequenceComplete") {
        // Prevent duplicate notifications
        if (lastNotifiedSequenceId.current !== SequenceId) {
          lastNotifiedSequenceId.current = SequenceId;

          // Get stored sequence name
          const sequenceName = sequenceNames.current.get(SequenceId);
          if (sequenceName) {
            addNotification({
              message: `Acquisition ${sequenceName} completed.`,
              type: "success",
            });

            // Clean up stored sequence name
            setTimeout(() => {
              sequenceNames.current.delete(SequenceId);
            }, 1000);
          }
        }
      }
    });

    return unsubscribe;
  }, [addNotification, subscribeToSequenceUpdates]);

  return (
    <NotificationContext.Provider
      value={{
        notifications,
        unreadCount,
        markAllAsRead,
        addNotification,
        clearNotifications,
        registerSequence,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
};
