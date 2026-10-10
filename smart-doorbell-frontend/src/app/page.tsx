'use client';

import React, { useState } from 'react';
import { Shield, Bell, Radio, Camera, RefreshCw, CheckCircle2 } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { useHome } from '@/context/HomeContext';
import { useSignalR } from '@/context/SignalRContext';
import { DeviceStatusBadge } from '@/components/DeviceStatusBadge';
import { VisitorPhotoCard } from '@/components/VisitorPhotoCard';
import { api } from '@/lib/api';

export default function DashboardPage() {
  const { user } = useAuth();
  const { activeHome, loading: homeLoading, refreshHomes } = useHome();
  const { notificationsHistory, isConnected } = useSignalR();
  const [triggeringRing, setTriggeringRing] = useState(false);
  const [ringStatus, setRingStatus] = useState<string | null>(null);

  const buttonDevice = activeHome?.devices.find((d) => d.type === 'Button');

  const handleSimulateRing = async () => {
    if (!buttonDevice) {
      alert('No Button Device found for active home.');
      return;
    }

    setTriggeringRing(true);
    setRingStatus(null);
    try {
      const response = await api.post('/doorbell/ring', {
        buttonDeviceId: buttonDevice.id,
        deviceEventId: `web_evt_${Date.now()}`,
      });
      setRingStatus(`Ring Event Triggered Successfully! Session ID: ${response.data.sessionId}`);
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : String(err);
      console.error('Failed to trigger ring event:', err);
      setRingStatus(`Error triggering ring: ${errorMsg}`);
    } finally {
      setTriggeringRing(false);
    }
  };

  if (homeLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-20 space-y-4">
        <RefreshCw className="h-8 w-8 text-blue-500 animate-spin" />
        <p className="text-slate-400 text-sm">Loading smart doorbell dashboard...</p>
      </div>
    );
  }

  if (!activeHome) {
    return (
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-10 text-center max-w-xl mx-auto space-y-4">
        <Shield className="h-12 w-12 text-slate-600 mx-auto" />
        <h2 className="text-xl font-bold text-white">No Home Selected</h2>
        <p className="text-slate-400 text-sm">
          You are currently logged in as <strong className="text-blue-400">{user?.displayName}</strong>. Create or select a home to manage smart doorbell devices.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      
      {/* Top Welcome Banner & Simulation Bar */}
      <div className="bg-gradient-to-r from-slate-900 via-slate-900 to-blue-950 border border-slate-800 rounded-2xl p-6 sm:p-8 flex flex-col md:flex-row items-start md:items-center justify-between gap-6 shadow-xl">
        <div className="space-y-2">
          <div className="flex items-center space-x-3">
            <span className="bg-blue-500/20 text-blue-400 text-xs font-semibold px-3 py-1 rounded-full border border-blue-500/30">
              Active Home
            </span>
            <span className="text-xs text-slate-400">
              ID: <code className="font-mono text-slate-300">{activeHome.id.substring(0, 8)}...</code>
            </span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">{activeHome.name}</h1>
          <p className="text-sm text-slate-400">
            Welcome back, <span className="text-slate-200 font-medium">{user?.displayName}</span>. All smart devices are monitored in real time via SignalR.
          </p>
        </div>

        {/* Real-time Ring Simulator Button */}
        <div className="flex flex-col items-stretch md:items-end space-y-2 w-full md:w-auto">
          <button
            onClick={handleSimulateRing}
            disabled={triggeringRing || !buttonDevice}
            className="flex items-center justify-center space-x-2 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-semibold px-6 py-3 rounded-xl transition shadow-lg shadow-blue-900/40 disabled:opacity-50"
          >
            <Bell className={`h-5 w-5 ${triggeringRing ? 'animate-bounce' : ''}`} />
            <span>{triggeringRing ? 'Pressing Doorbell...' : 'Ring Doorbell (Simulate)'}</span>
          </button>
          {ringStatus && (
            <span className="text-xs text-emerald-400 font-medium flex items-center space-x-1">
              <CheckCircle2 className="h-3.5 w-3.5" />
              <span>{ringStatus}</span>
            </span>
          )}
        </div>
      </div>

      {/* Connected Devices Grid */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-bold text-slate-200 flex items-center space-x-2">
            <Radio className="h-5 w-5 text-blue-400" />
            <span>Smart Devices Status</span>
          </h2>
          <button
            onClick={() => refreshHomes()}
            className="text-xs text-slate-400 hover:text-blue-400 flex items-center space-x-1 transition"
          >
            <RefreshCw className="h-3.5 w-3.5" />
            <span>Refresh Devices</span>
          </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {activeHome.devices.map((device) => (
            <DeviceStatusBadge key={device.id} device={device} />
          ))}
          {activeHome.devices.length === 0 && (
            <div className="col-span-3 bg-slate-900/50 border border-slate-800 rounded-xl p-6 text-center text-slate-400 text-sm">
              No devices paired with this home yet. Run the simulators to automatically pair test devices!
            </div>
          )}
        </div>
      </div>

      {/* Live Ring Notifications Feed */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-bold text-slate-200 flex items-center space-x-2">
            <Camera className="h-5 w-5 text-indigo-400" />
            <span>Live Visitor Notifications & Photos</span>
          </h2>
          <span className="text-xs text-slate-400 font-mono">
            {notificationsHistory.length} event(s) captured
          </span>
        </div>

        {notificationsHistory.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {notificationsHistory.map((item) => (
              <VisitorPhotoCard
                key={item.notificationId || item.timestamp}
                imageUrl={item.imageUrl}
                timestamp={item.timestamp}
                pressCount={item.pressCount}
              />
            ))}
          </div>
        ) : (
          <div className="bg-slate-900/60 border border-slate-800/80 rounded-2xl p-8 text-center space-y-3">
            <Bell className="h-10 w-10 text-slate-600 mx-auto" />
            <h3 className="font-semibold text-slate-300">No Ring Notifications Yet</h3>
            <p className="text-xs text-slate-400 max-w-md mx-auto">
              Press the <strong className="text-blue-400">Ring Doorbell</strong> button above or trigger a ring event from the <code className="bg-slate-800 text-slate-200 px-1 py-0.5 rounded">ButtonSimulator</code> to test live notifications!
            </p>
          </div>
        )}
      </div>

    </div>
  );
}

