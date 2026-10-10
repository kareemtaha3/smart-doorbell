import type { Metadata } from 'next';
import './globals.css';
import { AuthProvider } from '@/context/AuthContext';
import { HomeProvider } from '@/context/HomeContext';
import { SignalRProvider } from '@/context/SignalRContext';
import { Navbar } from '@/components/Navbar';
import { LiveRingModal } from '@/components/LiveRingModal';

export const metadata: Metadata = {
  title: 'Smart Doorbell Dashboard',
  description: 'Real-time smart doorbell system monitoring and management dashboard.',
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="bg-slate-950 text-slate-100 min-h-screen flex flex-col antialiased" suppressHydrationWarning>
        <AuthProvider>
          <HomeProvider>
            <SignalRProvider>
              <Navbar />
              <LiveRingModal />
              <main className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {children}
              </main>
              <footer className="bg-slate-900 border-t border-slate-800 text-slate-500 py-6 text-center text-xs">
                Smart Doorbell System &copy; {new Date().getFullYear()} — Built with Next.js, SignalR & ASP.NET Core
              </footer>
            </SignalRProvider>
          </HomeProvider>
        </AuthProvider>
      </body>
    </html>
  );
}

