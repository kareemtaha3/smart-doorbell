'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Bell, Home as HomeIcon, Music, Users, History, Radio, Shield, User as UserIcon } from 'lucide-react';
import { useAuth, SEEDED_USERS } from '@/context/AuthContext';
import { useHome } from '@/context/HomeContext';
import { useSignalR } from '@/context/SignalRContext';
import { subscribeToWebPush, isPushSubscribed } from '@/lib/webPush';

export const Navbar: React.FC = () => {
  const pathname = usePathname();
  const { user, switchUser } = useAuth();
  const { homes, activeHome, setActiveHomeId } = useHome();
  const { isConnected } = useSignalR();
  const [pushSubscribed, setPushSubscribed] = useState(false);
  const [subscribing, setSubscribing] = useState(false);
  const [mounted, setMounted] = useState(false);

  React.useEffect(() => {
    setMounted(true);
    let isCancelled = false;
    isPushSubscribed().then((subscribed) => {
      if (!isCancelled) {
        setPushSubscribed(subscribed);
      }
    });
    return () => {
      isCancelled = true;
    };
  }, [user]);

  const handleEnablePush = async () => {
    if (!user) return;
    setSubscribing(true);
    try {
      const success = await subscribeToWebPush(user.id);
      setPushSubscribed(success);
    } catch (err) {
      console.error('Failed to enable push notifications:', err);
      setPushSubscribed(false);
    } finally {
      setSubscribing(false);
    }
  };

  const navLinks = [
    { href: '/', label: 'Dashboard', icon: HomeIcon },
    { href: '/homes', label: 'Homes', icon: Shield },
    { href: '/members', label: 'Members', icon: Users },
    { href: '/melodies', label: 'Melodies', icon: Music },
    { href: '/history', label: 'History', icon: History },
  ];

  return (
    <nav className="bg-slate-900 border-b border-slate-800 text-white sticky top-0 z-40">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Brand & Active Home Selector */}
          <div className="flex items-center space-x-6">
            <Link href="/" className="flex items-center space-x-2 text-xl font-bold tracking-wider text-blue-400">
              <Bell className="h-6 w-6 text-blue-500 animate-pulse" />
              <span>SmartDoorbell</span>
            </Link>

            {mounted && homes.length > 0 && (
              <div className="relative">
                <select
                  value={activeHome?.id || ''}
                  onChange={(e) => setActiveHomeId(e.target.value)}
                  className="bg-slate-800 border border-slate-700 text-slate-200 text-sm rounded-lg px-3 py-1.5 focus:ring-2 focus:ring-blue-500 focus:outline-none"
                >
                  {homes.map((h) => (
                    <option key={h.id} value={h.id}>
                      🏠 {h.name}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          {/* Nav Links */}
          <div className="hidden md:flex items-center space-x-1">
            {navLinks.map((link) => {
              const Icon = link.icon;
              const isActive = pathname === link.href;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className={`flex items-center space-x-2 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                    isActive
                      ? 'bg-blue-600 text-white'
                      : 'text-slate-300 hover:bg-slate-800 hover:text-white'
                  }`}
                >
                  <Icon className="h-4 w-4" />
                  <span>{link.label}</span>
                </Link>
              );
            })}
          </div>

          {/* Right Controls: SignalR Badge, Push Toggle, User Switcher */}
          <div className="flex items-center space-x-4">
            {/* Live Connection Badge */}
            <div
              className={`flex items-center space-x-1.5 text-xs font-semibold px-2.5 py-1 rounded-full border ${
                isConnected
                  ? 'bg-emerald-950/80 text-emerald-400 border-emerald-800'
                  : 'bg-amber-950/80 text-amber-400 border-amber-800'
              }`}
              title={isConnected ? 'SignalR Live Connected' : 'Connecting to SignalR...'}
            >
              <Radio className={`h-3.5 w-3.5 ${isConnected ? 'animate-ping' : ''}`} />
              <span>{isConnected ? 'LIVE' : 'OFFLINE'}</span>
            </div>

            {/* Web Push Notification Button */}
            <button
              onClick={handleEnablePush}
              disabled={subscribing || pushSubscribed}
              className={`flex items-center space-x-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition ${
                pushSubscribed
                  ? 'bg-slate-800 text-slate-400 cursor-default'
                  : 'bg-blue-600 hover:bg-blue-500 text-white'
              }`}
            >
              <Bell className="h-3.5 w-3.5" />
              <span>{pushSubscribed ? 'Push Active' : subscribing ? 'Enabling...' : 'Enable Web Push'}</span>
            </button>

            {/* Quick Test User Switcher */}
            {mounted && (
              <div className="flex items-center space-x-2 bg-slate-800 border border-slate-700 rounded-lg p-1">
                <UserIcon className="h-4 w-4 text-slate-400 ml-1.5" />
                <select
                  value={user?.id || ''}
                  onChange={(e) => switchUser(e.target.value)}
                  className="bg-transparent text-xs text-slate-200 font-medium py-1 px-1 focus:outline-none cursor-pointer"
                >
                  {SEEDED_USERS.map((u) => (
                    <option key={u.id} value={u.id} className="bg-slate-900 text-slate-200">
                      {u.displayName} ({u.role})
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};

