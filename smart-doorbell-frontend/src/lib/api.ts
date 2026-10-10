import axios from 'axios';
import { Home, Melody, Invitation, User, RingSessionDetail } from './types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5147/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const authApi = {
  login: async (usernameOrEmail: string) => {
    const response = await api.post<User>('/auth/login', { usernameOrEmail, password: 'Password123!' });
    return response.data;
  },
  getUserById: async (id: string) => {
    const response = await api.get<User>(`/auth/${id}`);
    return response.data;
  },
};

export const homesApi = {
  getUserHomes: async (userId: string) => {
    const response = await api.get<Home[]>(`/homes/user/${userId}`);
    return response.data;
  },
  getHomeById: async (homeId: string) => {
    const response = await api.get<Home>(`/homes/${homeId}`);
    return response.data;
  },
  createHome: async (name: string, ownerUserId: string) => {
    const response = await api.post<Home>('/homes', { name, ownerUserId });
    return response.data;
  },
  updateHome: async (homeId: string, name: string) => {
    const response = await api.put<Home>(`/homes/${homeId}`, { name });
    return response.data;
  },
  deleteHome: async (homeId: string) => {
    await api.delete(`/homes/${homeId}`);
  },
  removeMember: async (homeId: string, userId: string) => {
    await api.delete(`/homes/${homeId}/members/${userId}`);
  },
};

export const melodiesApi = {
  getHomeMelodies: async (homeId: string) => {
    const response = await api.get<Melody[]>(`/homes/${homeId}/melodies`);
    return response.data;
  },
  uploadMelody: async (homeId: string, userId: string, name: string, isMain: boolean, file: File) => {
    const formData = new FormData();
    formData.append('name', name);
    formData.append('isMain', String(isMain));
    formData.append('file', file);

    const response = await api.post<Melody>(`/homes/${homeId}/melodies?userId=${userId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
  setMainMelody: async (homeId: string, melodyId: string, userId: string) => {
    const response = await api.put<Melody>(`/homes/${homeId}/melodies/${melodyId}/set-main?userId=${userId}`);
    return response.data;
  },
};

export const invitationsApi = {
  sendInvitation: async (homeId: string, invitedByUserId: string, invitedUsername: string) => {
    const response = await api.post<Invitation>(`/homes/${homeId}/invitations?invitedByUserId=${invitedByUserId}`, {
      invitedUsername,
    });
    return response.data;
  },
  getUserInvitations: async (userId: string) => {
    const response = await api.get<Invitation[]>(`/invitations?userId=${userId}`);
    return response.data;
  },
  respondToInvitation: async (invitationId: string, userId: string, accept: boolean) => {
    const response = await api.post<Invitation>(`/invitations/${invitationId}/respond?userId=${userId}`, {
      accept,
    });
    return response.data;
  },
};

export const pushApi = {
  getVapidPublicKey: async () => {
    const response = await api.get<{ publicKey: string }>('/push-subscriptions/vapid-public-key');
    return response.data.publicKey;
  },
  subscribe: async (userId: string, subscription: PushSubscriptionJSON) => {
    const response = await api.post(`/push-subscriptions?userId=${userId}`, {
      endpoint: subscription.endpoint,
      p256dhKey: subscription.keys?.p256dh,
      authKey: subscription.keys?.auth,
    });
    return response.data;
  },
  unsubscribe: async (userId: string, endpoint: string) => {
    await api.delete(`/push-subscriptions?userId=${userId}&endpoint=${encodeURIComponent(endpoint)}`);
  },
};

export const doorbellApi = {
  markNotificationRead: async (notificationId: string, userId: string, channel: string = 'SignalR') => {
    await api.post(`/notifications/${notificationId}/read?userId=${userId}&channel=${channel}`);
  },
  closeSession: async (sessionId: string) => {
    await api.post(`/doorbell/sessions/${sessionId}/close`);
  },
  getSessionDetails: async (sessionId: string) => {
    const response = await api.get<RingSessionDetail>(`/sessions/${sessionId}`);
    return response.data;
  }
};
