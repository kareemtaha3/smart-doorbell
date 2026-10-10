'use client';

import React from 'react';
import { Wifi, WifiOff, Camera, Bell, Radio } from 'lucide-react';
import { Device } from '@/lib/types';

export const DeviceStatusBadge: React.FC<{ device: Device }> = ({ device }) => {
  const isOnline = device.status === 'Online';

  const getIcon = () => {
    switch (device.type) {
      case 'Button':
        return Radio;
      case 'Camera':
        return Camera;
      case 'Chime':
        return Bell;
      default:
        return Wifi;
    }
  };

  const DeviceIcon = getIcon();

  return (
    <div className="bg-slate-800/90 border border-slate-700/80 rounded-xl p-4 flex items-center justify-between space-x-4">
      <div className="flex items-center space-x-3">
        <div className="p-2.5 rounded-lg bg-slate-900 border border-slate-700">
          <DeviceIcon className="h-5 w-5 text-blue-400" />
        </div>
        <div>
          <h4 className="font-semibold text-slate-200 text-sm">{device.type} Device</h4>
          <p className="text-xs text-slate-400 font-mono">{device.id.substring(0, 18)}...</p>
        </div>
      </div>

      <div className="flex items-center space-x-2">
        <span
          className={`flex items-center space-x-1 px-2.5 py-1 rounded-full text-xs font-semibold border ${
            isOnline
              ? 'bg-emerald-950/80 text-emerald-400 border-emerald-800'
              : 'bg-rose-950/80 text-rose-400 border-rose-800'
          }`}
        >
          {isOnline ? <Wifi className="h-3 w-3" /> : <WifiOff className="h-3 w-3" />}
          <span>{device.status}</span>
        </span>
      </div>
    </div>
  );
};

