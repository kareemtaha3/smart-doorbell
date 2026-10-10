'use client';

import React, { createContext, useContext, useEffect, useState } from 'react';
import { User } from '@/lib/types';
import { authApi } from '@/lib/api';

export const SEEDED_USERS = [
  { id: '11111111-1111-1111-1111-111111111111', username: 'homeowner', displayName: 'Main Home Owner', role: 'Owner' },
  { id: '22222222-2222-2222-2222-222222222222', username: 'familymember', displayName: 'Family Member', role: 'Member' },
];

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (usernameOrEmail: string) => Promise<void>;
  switchUser: (userId: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  loading: true,
  login: async () => {},
  switchUser: async () => {},
  logout: () => {},
});

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedUserId = localStorage.getItem('smart_doorbell_user_id');
    if (storedUserId) {
      authApi
        .getUserById(storedUserId)
        .then((fetchedUser) => {
          setUser(fetchedUser);
        })
        .catch(() => {
          // Default to owner user if stored id fails
          switchUser(SEEDED_USERS[0].id);
        })
        .finally(() => setLoading(false));
    } else {
      // Default to homeowner for initial quick test
      switchUser(SEEDED_USERS[0].id).finally(() => setLoading(false));
    }
  }, []);

  const login = async (usernameOrEmail: string) => {
    setLoading(true);
    try {
      const loggedUser = await authApi.login(usernameOrEmail);
      setUser(loggedUser);
      localStorage.setItem('smart_doorbell_user_id', loggedUser.id);
    } finally {
      setLoading(false);
    }
  };

  const switchUser = async (userId: string) => {
    setLoading(true);
    try {
      const fetchedUser = await authApi.getUserById(userId);
      setUser(fetchedUser);
      localStorage.setItem('smart_doorbell_user_id', fetchedUser.id);
    } catch {
      const fallback = SEEDED_USERS.find((u) => u.id === userId);
      if (fallback) {
        const mockUser: User = {
          id: fallback.id,
          username: fallback.username,
          email: `${fallback.username}@smartdoorbell.local`,
          displayName: fallback.displayName,
          createdAt: new Date().toISOString(),
        };
        setUser(mockUser);
        localStorage.setItem('smart_doorbell_user_id', mockUser.id);
      }
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('smart_doorbell_user_id');
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, switchUser, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);

