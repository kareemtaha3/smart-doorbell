'use client';

import React, { useEffect, useState } from 'react';
import { Users, UserPlus, Mail, Check, X, Shield, Trash2 } from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { useHome } from '@/context/HomeContext';
import { invitationsApi, homesApi } from '@/lib/api';
import { Invitation } from '@/lib/types';

export default function MembersPage() {
  const { user } = useAuth();
  const { activeHome, refreshHomes } = useHome();
  const [invitedUsername, setInvitedUsername] = useState('');
  const [sending, setSending] = useState(false);
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [loadingInvitations, setLoadingInvitations] = useState(true);

  const fetchInvitations = async () => {
    if (!user) return;
    setLoadingInvitations(true);
    try {
      const list = await invitationsApi.getUserInvitations(user.id);
      setInvitations(list);
    } catch (err) {
      console.error('Failed to fetch invitations:', err);
    } finally {
      setLoadingInvitations(false);
    }
  };

  useEffect(() => {
    fetchInvitations();
  }, [user?.id]);

  const handleSendInvitation = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!invitedUsername.trim() || !activeHome || !user) return;

    setSending(true);
    try {
      await invitationsApi.sendInvitation(activeHome.id, user.id, invitedUsername.trim());
      alert(`Invitation sent to '${invitedUsername}'!`);
      setInvitedUsername('');
      await fetchInvitations();
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : String(err);
      console.error('Error sending invitation:', err);
      alert(`Error sending invitation: ${errorMsg}`);
    } finally {
      setSending(false);
    }
  };

  const handleRespond = async (invitationId: string, accept: boolean) => {
    if (!user) return;
    try {
      await invitationsApi.respondToInvitation(invitationId, user.id, accept);
      await fetchInvitations();
      await refreshHomes();
    } catch (err) {
      console.error('Error responding to invitation:', err);
    }
  };

  const handleRemoveMember = async (userId: string) => {
    if (!activeHome || !confirm('Remove this member from the home?')) return;
    try {
      await homesApi.removeMember(activeHome.id, userId);
      await refreshHomes();
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : String(err);
      alert(`Could not remove member: ${errorMsg}`);
    }
  };

  if (!activeHome) {
    return (
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-10 text-center max-w-lg mx-auto space-y-3">
        <Users className="h-10 w-10 text-slate-600 mx-auto" />
        <h2 className="text-lg font-bold text-white">No Active Home</h2>
        <p className="text-sm text-slate-400">Please select an active home to manage family members and invitations.</p>
      </div>
    );
  }

  return (
    <div className="space-y-8 max-w-5xl mx-auto">
      <div>
        <h1 className="text-2xl font-extrabold text-white flex items-center space-x-2">
          <Users className="h-7 w-7 text-blue-500" />
          <span>Members & Home Access</span>
        </h1>
        <p className="text-slate-400 text-sm">
          Manage family members for <strong className="text-blue-400">{activeHome.name}</strong> and process pending invitations.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        
        {/* Left Column: Active Members List & Invite Form */}
        <div className="space-y-6">
          {/* Invite Form */}
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4 shadow-lg">
            <h2 className="text-base font-bold text-slate-200 flex items-center space-x-2">
              <UserPlus className="h-5 w-5 text-blue-400" />
              <span>Invite Family Member</span>
            </h2>
            <form onSubmit={handleSendInvitation} className="space-y-3">
              <input
                type="text"
                placeholder="Enter username (e.g. familymember)"
                value={invitedUsername}
                onChange={(e) => setInvitedUsername(e.target.value)}
                className="w-full bg-slate-950 border border-slate-700 text-slate-100 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              />
              <button
                type="submit"
                disabled={sending}
                className="w-full bg-blue-600 hover:bg-blue-500 text-white font-semibold py-2.5 rounded-xl text-sm transition disabled:opacity-50"
              >
                {sending ? 'Sending Invitation...' : 'Send Home Invitation'}
              </button>
            </form>
          </div>

          {/* Members List */}
          <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4">
            <h2 className="text-base font-bold text-slate-200">Current Members ({activeHome.members.length})</h2>

            <div className="space-y-3">
              {activeHome.members.map((member) => (
                <div
                  key={member.id}
                  className="bg-slate-950 border border-slate-800/80 rounded-xl p-4 flex items-center justify-between"
                >
                  <div className="space-y-0.5">
                    <div className="flex items-center space-x-2">
                      <span className="font-semibold text-slate-200 text-sm">{member.displayName || member.username}</span>
                      <span
                        className={`text-[10px] uppercase font-bold px-2 py-0.5 rounded-full ${
                          member.role === 'Owner'
                            ? 'bg-amber-950 text-amber-400 border border-amber-800'
                            : 'bg-slate-800 text-slate-300'
                        }`}
                      >
                        {member.role}
                      </span>
                    </div>
                    <span className="text-xs text-slate-500 font-mono">@{member.username}</span>
                  </div>

                  {member.role !== 'Owner' && (
                    <button
                      onClick={() => handleRemoveMember(member.userId)}
                      className="text-rose-400 hover:text-rose-300 p-1.5 rounded-lg hover:bg-slate-900 transition"
                      title="Remove Member"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Right Column: Pending Invitations Inbox */}
        <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 space-y-4 self-start">
          <h2 className="text-base font-bold text-slate-200 flex items-center space-x-2">
            <Mail className="h-5 w-5 text-indigo-400" />
            <span>Your Invitations Inbox</span>
          </h2>

          {loadingInvitations ? (
            <p className="text-xs text-slate-400">Loading invitations...</p>
          ) : invitations.length > 0 ? (
            <div className="space-y-3">
              {invitations.map((inv) => (
                <div key={inv.id} className="bg-slate-950 border border-slate-800 rounded-xl p-4 space-y-3">
                  <div>
                    <h3 className="font-semibold text-slate-200 text-sm">Home: {inv.homeName}</h3>
                    <p className="text-xs text-slate-400">
                      Invited by <strong className="text-slate-200">@{inv.invitedByUsername}</strong>
                    </p>
                  </div>

                  {inv.status === 'Pending' ? (
                    <div className="flex items-center space-x-2 pt-1">
                      <button
                        onClick={() => handleRespond(inv.id, true)}
                        className="flex-1 bg-emerald-600 hover:bg-emerald-500 text-white font-semibold py-1.5 rounded-lg text-xs flex items-center justify-center space-x-1"
                      >
                        <Check className="h-3.5 w-3.5" />
                        <span>Accept</span>
                      </button>
                      <button
                        onClick={() => handleRespond(inv.id, false)}
                        className="flex-1 bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold py-1.5 rounded-lg text-xs flex items-center justify-center space-x-1"
                      >
                        <X className="h-3.5 w-3.5" />
                        <span>Decline</span>
                      </button>
                    </div>
                  ) : (
                    <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider block">
                      Status: {inv.status}
                    </span>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center p-6 text-slate-500 text-xs space-y-1">
              <Shield className="h-6 w-6 mx-auto opacity-40" />
              <p>No pending home invitations.</p>
            </div>
          )}
        </div>

      </div>
    </div>
  );
}

