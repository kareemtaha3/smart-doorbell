'use client';

import React from 'react';
import { History, Camera, Clock, Layers, ShieldCheck } from 'lucide-react';
import { useSignalR } from '@/context/SignalRContext';
import { useHome } from '@/context/HomeContext';
import { VisitorPhotoCard } from '@/components/VisitorPhotoCard';

export default function HistoryPage() {
  const { activeHome } = useHome();
  const { notificationsHistory } = useSignalR();

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      <div>
        <h1 className="text-2xl font-extrabold text-white flex items-center space-x-2">
          <History className="h-7 w-7 text-blue-500" />
          <span>Doorbell Ring & Visitor History</span>
        </h1>
        <p className="text-slate-400 text-sm">
          View past ring events, press counts, and visitor snapshot gallery captured for <strong className="text-blue-400">{activeHome?.name || 'Active Home'}</strong>.
        </p>
      </div>

      {notificationsHistory.length > 0 ? (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {notificationsHistory.map((item) => (
              <div key={item.notificationId || item.timestamp} className="bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden space-y-3 p-4">
                <VisitorPhotoCard
                  imageUrl={item.imageUrl}
                  timestamp={item.timestamp}
                  pressCount={item.pressCount}
                />

                <div className="space-y-1.5 pt-1 text-xs">
                  <div className="flex justify-between text-slate-300 font-semibold">
                    <span>{item.title}</span>
                    <span className="text-blue-400 font-mono">#{item.sessionId.substring(0, 6)}</span>
                  </div>
                  <p className="text-slate-400">{item.body}</p>

                  <div className="flex items-center justify-between text-[11px] text-slate-500 pt-2 border-t border-slate-800">
                    <span className="flex items-center space-x-1">
                      <Clock className="h-3 w-3" />
                      <span>{new Date(item.timestamp).toLocaleTimeString()}</span>
                    </span>
                    <span className="flex items-center space-x-1 text-emerald-400">
                      <ShieldCheck className="h-3 w-3" />
                      <span>Delivered</span>
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-12 text-center space-y-3 max-w-lg mx-auto">
          <Camera className="h-12 w-12 text-slate-600 mx-auto" />
          <h3 className="font-bold text-slate-300 text-base">No History Records Found</h3>
          <p className="text-xs text-slate-400">
            When visitors press the doorbell, captured snapshots and session details will automatically accumulate in this feed.
          </p>
        </div>
      )}
    </div>
  );
}

