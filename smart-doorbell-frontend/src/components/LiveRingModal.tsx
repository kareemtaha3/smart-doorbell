'use client';

import React from 'react';
import { Bell, CheckCircle, XCircle, Camera, Clock, Layers } from 'lucide-react';
import { useSignalR } from '@/context/SignalRContext';

export const LiveRingModal: React.FC = () => {
  const { activeAlert, silenceCountdown, dismissAlert, closeActiveSession } = useSignalR();

  if (!activeAlert) return null;

  const progressPercent = (silenceCountdown / 15) * 100;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/75 backdrop-blur-md p-4 animate-in fade-in duration-200">
      <div className="bg-slate-900 border-2 border-blue-500/80 rounded-2xl shadow-2xl max-w-lg w-full overflow-hidden text-white transform transition-all scale-100">
        
        {/* Header Alert Bar */}
        <div className="bg-gradient-to-r from-red-600 to-amber-600 px-6 py-4 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <div className="bg-white/20 p-2 rounded-full animate-bounce">
              <Bell className="h-6 w-6 text-white" />
            </div>
            <div>
              <h2 className="font-bold text-lg leading-tight">{activeAlert.title}</h2>
              <p className="text-xs text-white/80">{activeAlert.body}</p>
            </div>
          </div>
          <span className="bg-white/20 text-xs px-3 py-1 rounded-full font-mono font-bold uppercase tracking-wider">
            LIVE RINGING
          </span>
        </div>

        {/* Modal Body Content */}
        <div className="p-6 space-y-5">
          
          {/* Press Counter & Timers */}
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-slate-800/90 border border-slate-700/80 rounded-xl p-3 flex items-center space-x-3">
              <Layers className="h-5 w-5 text-amber-400" />
              <div>
                <span className="text-xs text-slate-400 block font-medium">Press Count</span>
                <span className="text-lg font-bold text-amber-300">{activeAlert.pressCount} Press(es)</span>
              </div>
            </div>

            <div className="bg-slate-800/90 border border-slate-700/80 rounded-xl p-3 flex items-center space-x-3">
              <Clock className="h-5 w-5 text-blue-400" />
              <div>
                <span className="text-xs text-slate-400 block font-medium">Silence Timer</span>
                <span className="text-lg font-bold text-blue-300">{silenceCountdown}s Window</span>
              </div>
            </div>
          </div>

          {/* 15s Silence Countdown Bar */}
          <div>
            <div className="flex justify-between text-xs text-slate-400 mb-1">
              <span>Auto-close on 15s silence</span>
              <span>{silenceCountdown}s remaining</span>
            </div>
            <div className="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
              <div
                className="bg-blue-500 h-full transition-all duration-1000 ease-linear"
                style={{ width: `${progressPercent}%` }}
              />
            </div>
          </div>

          {/* Visitor Snapshot Display */}
          <div className="bg-slate-950 border border-slate-800 rounded-xl overflow-hidden aspect-video relative flex items-center justify-center">
            {activeAlert.imageUrl ? (
              <img
                src={activeAlert.imageUrl}
                alt="Visitor Camera Snapshot"
                className="w-full h-full object-cover rounded-xl"
              />
            ) : (
              <div className="text-center p-6 space-y-2">
                <Camera className="h-10 w-10 text-slate-600 mx-auto animate-pulse" />
                <p className="text-sm font-medium text-slate-400">Requesting Outdoor Camera Snapshot...</p>
                <p className="text-xs text-slate-500">MinIO webhook will display visitor photo here upon upload.</p>
              </div>
            )}
          </div>

          {/* Action Buttons */}
          <div className="flex items-center justify-end space-x-3 pt-2">
            <button
              onClick={dismissAlert}
              className="flex items-center space-x-2 px-4 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-sm font-semibold transition"
            >
              <CheckCircle className="h-4 w-4 text-emerald-400" />
              <span>Acknowledge</span>
            </button>

            <button
              onClick={closeActiveSession}
              className="flex items-center space-x-2 px-4 py-2.5 rounded-xl bg-red-600 hover:bg-red-500 text-white text-sm font-semibold transition shadow-lg shadow-red-900/40"
            >
              <XCircle className="h-4 w-4" />
              <span>End Ring Session</span>
            </button>
          </div>

        </div>

      </div>
    </div>
  );
};

