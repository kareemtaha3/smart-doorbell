'use client';

import React, { useState } from 'react';
import { Shield, Plus, Edit2, Trash2, Check, X } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { useHome } from '@/context/HomeContext';
import { homesApi } from '@/lib/api';

export default function HomesPage() {
  const { user } = useAuth();
  const { homes, activeHome, setActiveHomeId, refreshHomes } = useHome();
  const [newHomeName, setNewHomeName] = useState('');
  const [creating, setCreating] = useState(false);
  const [editingHomeId, setEditingHomeId] = useState<string | null>(null);
  const [editingName, setEditingName] = useState('');

  const handleCreateHome = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newHomeName.trim() || !user) return;

    setCreating(true);
    try {
      const created = await homesApi.createHome(newHomeName.trim(), user.id);
      setNewHomeName('');
      await refreshHomes();
      setActiveHomeId(created.id);
    } catch (err) {
      console.error('Failed to create home:', err);
      alert('Error creating home.');
    } finally {
      setCreating(false);
    }
  };

  const handleUpdateHome = async (homeId: string) => {
    if (!editingName.trim()) return;
    try {
      await homesApi.updateHome(homeId, editingName.trim());
      setEditingHomeId(null);
      await refreshHomes();
    } catch (err) {
      console.error('Failed to update home:', err);
    }
  };

  const handleDeleteHome = async (homeId: string) => {
    if (!confirm('Are you sure you want to delete this home and all attached device records?')) return;
    try {
      await homesApi.deleteHome(homeId);
      await refreshHomes();
    } catch (err) {
      console.error('Failed to delete home:', err);
    }
  };

  return (
    <div className="space-y-8 max-w-5xl mx-auto">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold text-white flex items-center space-x-2">
            <Shield className="h-7 w-7 text-blue-500" />
            <span>Homes Management</span>
          </h1>
          <p className="text-slate-400 text-sm">Create, rename, or switch between smart homes.</p>
        </div>
      </div>

      {/* Create New Home Form */}
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-lg">
        <h2 className="text-base font-bold text-slate-200 mb-4 flex items-center space-x-2">
          <Plus className="h-5 w-5 text-blue-400" />
          <span>Create New Smart Home</span>
        </h2>
        <form onSubmit={handleCreateHome} className="flex flex-col sm:flex-row gap-3">
          <input
            type="text"
            placeholder="e.g. Vacation House, Beach Villa, Main Home"
            value={newHomeName}
            onChange={(e) => setNewHomeName(e.target.value)}
            className="flex-1 bg-slate-950 border border-slate-700 text-slate-100 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
          <button
            type="submit"
            disabled={creating}
            className="bg-blue-600 hover:bg-blue-500 text-white font-semibold px-6 py-2.5 rounded-xl text-sm transition disabled:opacity-50"
          >
            {creating ? 'Creating...' : 'Create Home'}
          </button>
        </form>
      </div>

      {/* Homes List */}
      <div className="space-y-4">
        <h2 className="text-base font-bold text-slate-300">Your Homes ({homes.length})</h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {homes.map((home) => {
            const isSelected = activeHome?.id === home.id;
            const isEditing = editingHomeId === home.id;

            return (
              <div
                key={home.id}
                className={`bg-slate-900 border rounded-2xl p-6 space-y-4 transition ${
                  isSelected ? 'border-blue-500 ring-2 ring-blue-500/20 shadow-blue-900/20' : 'border-slate-800'
                }`}
              >
                <div className="flex items-start justify-between">
                  <div>
                    {isEditing ? (
                      <div className="flex items-center space-x-2">
                        <input
                          type="text"
                          value={editingName}
                          onChange={(e) => setEditingName(e.target.value)}
                          className="bg-slate-950 border border-slate-700 text-slate-100 text-sm rounded-lg px-3 py-1"
                        />
                        <button
                          onClick={() => handleUpdateHome(home.id)}
                          className="p-1 text-emerald-400 hover:text-emerald-300"
                        >
                          <Check className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setEditingHomeId(null)}
                          className="p-1 text-slate-400 hover:text-slate-300"
                        >
                          <X className="h-4 w-4" />
                        </button>
                      </div>
                    ) : (
                      <div className="flex items-center space-x-2">
                        <h3 className="font-bold text-white text-lg">{home.name}</h3>
                        <button
                          onClick={() => {
                            setEditingHomeId(home.id);
                            setEditingName(home.name);
                          }}
                          className="text-slate-500 hover:text-slate-300 p-1"
                        >
                          <Edit2 className="h-3.5 w-3.5" />
                        </button>
                      </div>
                    )}
                    <span className="text-xs text-slate-400 font-mono block mt-1">
                      ID: {home.id}
                    </span>
                  </div>

                  {isSelected ? (
                    <span className="bg-blue-500/20 text-blue-400 text-xs font-semibold px-2.5 py-1 rounded-full border border-blue-500/30">
                      Active Home
                    </span>
                  ) : (
                    <button
                      onClick={() => setActiveHomeId(home.id)}
                      className="bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs px-3 py-1.5 rounded-lg transition"
                    >
                      Set Active
                    </button>
                  )}
                </div>

                <div className="grid grid-cols-2 gap-2 text-xs bg-slate-950/60 p-3 rounded-xl text-slate-400 border border-slate-800/80">
                  <div>
                    <span className="text-slate-500 block">Members Count</span>
                    <span className="font-bold text-slate-200">{home.members.length} Member(s)</span>
                  </div>
                  <div>
                    <span className="text-slate-500 block">Devices Count</span>
                    <span className="font-bold text-slate-200">{home.devices.length} Device(s)</span>
                  </div>
                </div>

                <div className="flex justify-end pt-2">
                  <button
                    onClick={() => handleDeleteHome(home.id)}
                    className="text-xs text-rose-400 hover:text-rose-300 flex items-center space-x-1"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                    <span>Delete Home</span>
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

