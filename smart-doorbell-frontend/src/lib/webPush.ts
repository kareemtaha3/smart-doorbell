import { pushApi } from './api';

const DEFAULT_VAPID_PUBLIC_KEY = 'BJO9uIpbqPIkbwmU-zEC93B4rV3K-xjQePEmnK5tfpI3y4eLtA_OsaWILStUzJdxnya-ZbiGnTNOpndDGuaxTPc';

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');

  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);

  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

export async function registerServiceWorker(): Promise<ServiceWorkerRegistration | null> {
  if (typeof window === 'undefined' || !('serviceWorker' in navigator)) {
    console.warn('Service Worker is not supported in this browser environment.');
    return null;
  }

  try {
    const registration = await navigator.serviceWorker.register('/sw.js', {
      scope: '/',
    });
    console.log('[WebPush] Service Worker registered with scope:', registration.scope);
    await navigator.serviceWorker.ready;
    return registration;
  } catch (error) {
    console.warn('[WebPush] Service Worker registration failed:', error);
    return null;
  }
}

export async function subscribeToWebPush(userId: string): Promise<boolean> {
  if (typeof window === 'undefined' || !('PushManager' in window)) {
    console.warn('[WebPush] PushManager not supported in this browser.');
    return false;
  }

  try {
    const permission = await Notification.requestPermission();
    if (permission !== 'granted') {
      console.warn('[WebPush] User denied notification permission.');
      return false;
    }

    const registration = await registerServiceWorker();
    if (!registration) return false;

    // Ensure Service Worker is fully active before proceeding
    const readyRegistration = await navigator.serviceWorker.ready;

    let fetchedKey = '';
    try {
      fetchedKey = await pushApi.getVapidPublicKey();
    } catch (apiErr) {
      console.warn('[WebPush] Could not fetch VAPID key from backend, using default fallback key.', apiErr);
    }

    const validKey = (fetchedKey || DEFAULT_VAPID_PUBLIC_KEY).trim().replace(/^"+|"+$/g, '');
    console.log('[WebPush] Using VAPID Public Key:', validKey);

    const convertedPublicKey = urlBase64ToUint8Array(validKey);

    // Clear existing subscription if any to prevent key mismatch AbortErrors
    let subscription = await readyRegistration.pushManager.getSubscription();
    if (subscription) {
      console.log('[WebPush] Clearing existing push subscription...');
      try {
        await subscription.unsubscribe();
      } catch (unsubErr) {
        console.warn('[WebPush] Unsubscribe existing failed:', unsubErr);
      }
    }

    // Subscribe with PushManager
    subscription = await readyRegistration.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: convertedPublicKey as unknown as BufferSource,
    });

    const subJson = subscription.toJSON();
    console.log('[WebPush] Browser PushSubscription created successfully:', subJson);

    await pushApi.subscribe(userId, subJson);
    console.log('[WebPush] Web Push Subscription successfully registered in DB for user:', userId);
    return true;
  } catch (error: any) {
    if (error?.name === 'AbortError') {
      console.error(
        '[WebPush] Push service registration aborted by browser or push network. ' +
        'This can happen if browser push service (Google FCM / Mozilla Push) is blocked by an adblocker, VPN, or network settings, ' +
        'or if the service worker registration was interrupted. Error details:', error
      );
    } else {
      console.error('[WebPush] Web Push subscription failed:', error);
    }
    return false;
  }
}

export async function isPushSubscribed(): Promise<boolean> {
  if (typeof window === 'undefined' || !('serviceWorker' in navigator)) return false;
  try {
    const registration = await navigator.serviceWorker.getRegistration();
    if (!registration) return false;
    const subscription = await registration.pushManager.getSubscription();
    return !!subscription;
  } catch {
    return false;
  }
}
