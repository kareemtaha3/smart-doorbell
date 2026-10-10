'use client';

import React, { createContext, useContext, useEffect, useState } from 'react';
import { Home } from '@/lib/types';
import { homesApi } from '@/lib/api';
import { useAuth } from './AuthContext';

interface HomeContextType {
  homes: Home[];
  activeHome: Home | null;
  loading: boolean;
  setActiveHomeId: (homeId: string) => void;
  refreshHomes: () => Promise<void>;
}

const HomeContext = createContext<HomeContextType>({
  homes: [],
  activeHome: null,
  loading: true,
  setActiveHomeId: () => {},
  refreshHomes: async () => {},
});

export const HomeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  const [homes, setHomes] = useState<Home[]>([]);
  const [activeHomeId, setActiveHomeIdState] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshHomes = async () => {
    if (!user) {
      setHomes([]);
      setActiveHomeIdState(null);
      setLoading(false);
      return;
    }

    setLoading(true);
    try {
      const userHomes = await homesApi.getUserHomes(user.id);
      setHomes(userHomes);

      if (userHomes.length > 0) {
        const storedHomeId = localStorage.getItem(`active_home_${user.id}`);
        const found = userHomes.find((h) => h.id === storedHomeId);
        const targetId = found ? found.id : userHomes[0].id;
        setActiveHomeIdState(targetId);
      } else {
        setActiveHomeIdState(null);
      }
    } catch (error) {
      console.error('[HomeContext] Error fetching homes:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshHomes();
  }, [user?.id]);

  const setActiveHomeId = (homeId: string) => {
    setActiveHomeIdState(homeId);
    if (user) {
      localStorage.setItem(`active_home_${user.id}`, homeId);
    }
  };

  const activeHome = homes.find((h) => h.id === activeHomeId) || (homes.length > 0 ? homes[0] : null);

  return (
    <HomeContext.Provider
      value={{
        homes,
        activeHome,
        loading,
        setActiveHomeId,
        refreshHomes,
      }}
    >
      {children}
    </HomeContext.Provider>
  );
};

export const useHome = () => useContext(HomeContext);

