'use client';

import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { NotificationPayload } from '@/lib/types';
import { useAuth } from './AuthContext';
import { useHome } from './HomeContext';
import { doorbellApi } from '@/lib/api';

interface SignalRContextType {
  isConnected: boolean;
  activeAlert: NotificationPayload | null;
  silenceCountdown: number;
  dismissAlert: () => void;
  closeActiveSession: () => Promise<void>;
  notificationsHistory: NotificationPayload[];
}

const SignalRContext = createContext<SignalRContextType>({
  isConnected: false,
  activeAlert: null,
  silenceCountdown: 0,
  dismissAlert: () => {},
  closeActiveSession: async () => {},
  notificationsHistory: [],
});

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  const { activeHome } = useHome();
  const [isConnected, setIsConnected] = useState(false);
  const [activeAlert, setActiveAlert] = useState<NotificationPayload | null>(null);
  const [silenceCountdown, setSilenceCountdown] = useState<number>(0);
  const [notificationsHistory, setNotificationsHistory] = useState<NotificationPayload[]>([]);

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const timerRef = useRef<NodeJS.Timeout | null>(null);

  const startSilenceTimer = () => {
    if (timerRef.current) clearInterval(timerRef.current);
    setSilenceCountdown(15);

    timerRef.current = setInterval(() => {
      setSilenceCountdown((prev) => {
        if (prev <= 1) {
          if (timerRef.current) clearInterval(timerRef.current);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  useEffect(() => {
    if (!user) {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
        setIsConnected(false);
      }
      return;
    }

    const hubUrl = `http://localhost:5147/hubs/doorbell?userId=${user.id}`;
    console.log('[SignalR] Connecting to hub:', hubUrl);

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    newConnection
      .start()
      .then(() => {
        console.log('[SignalR] SignalR Connected successfully.');
        setIsConnected(true);

        if (activeHome) {
          newConnection.invoke('JoinHomeGroup', activeHome.id).catch((err) => {
            console.error('[SignalR] Failed to join home group:', err);
          });
        }
      })
      .catch((err) => {
        console.error('[SignalR] Connection failed:', err);
        setIsConnected(false);
      });

    newConnection.on('ReceiveNotification', (payload: NotificationPayload) => {
      console.log('[SignalR] Live Notification Received:', payload);

      setActiveAlert(payload);
      startSilenceTimer();

      setNotificationsHistory((prev) => {
        const filtered = prev.filter((item) => item.notificationId !== payload.notificationId);
        return [payload, ...filtered];
      });

      if (user?.id && payload.notificationId) {
        doorbellApi.markNotificationRead(payload.notificationId, user.id, 'SignalR').catch(console.error);
      }
    });

    connectionRef.current = newConnection;

    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
      newConnection.stop();
    };
  }, [user?.id]);

  useEffect(() => {
    if (connectionRef.current && isConnected && activeHome) {
      connectionRef.current.invoke('JoinHomeGroup', activeHome.id).catch(console.error);
    }
  }, [activeHome?.id, isConnected]);

  const dismissAlert = () => {
    setActiveAlert(null);
    if (timerRef.current) clearInterval(timerRef.current);
  };

  const closeActiveSession = async () => {
    if (activeAlert?.sessionId) {
      try {
        await doorbellApi.closeSession(activeAlert.sessionId);
      } catch (error) {
        console.error('Error closing session:', error);
      }
    }
    dismissAlert();
  };

  return (
    <SignalRContext.Provider
      value={{
        isConnected,
        activeAlert,
        silenceCountdown,
        dismissAlert,
        closeActiveSession,
        notificationsHistory,
      }}
    >
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => useContext(SignalRContext);
