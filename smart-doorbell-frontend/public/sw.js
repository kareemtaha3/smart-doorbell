// Web Push Notifications Service Worker for Smart Doorbell

self.addEventListener('install', function (event) {
  console.log('[Service Worker] Installing...');
  self.skipWaiting();
});

self.addEventListener('activate', function (event) {
  console.log('[Service Worker] Activating...');
  event.waitUntil(self.clients.claim());
});

self.addEventListener('push', function (event) {
  console.log('[Service Worker] Push event received:', event);

  let data = {};
  if (event.data) {
    try {
      data = event.data.json();
    } catch (e) {
      data = { title: 'Smart Doorbell', body: event.data.text() };
    }
  }

  const title = data.title || 'Someone is at the door!';
  const options = {
    body: data.body || 'Doorbell pressed',
    tag: data.sessionId || data.tag || 'doorbell-alert',
    renotify: true,
    requireInteraction: true,
    data: {
      url: data.sessionId ? `/?session=${data.sessionId}` : '/',
      sessionId: data.sessionId,
      homeId: data.homeId
    },
    vibrate: [200, 100, 200, 100, 200, 100, 400]
  };

  if (data.imageUrl && typeof data.imageUrl === 'string' && data.imageUrl.startsWith('http')) {
    options.image = data.imageUrl;
  }

  event.waitUntil(
    self.registration.showNotification(title, options).catch(function (err) {
      console.error('[Service Worker] showNotification failed with image, retrying text-only:', err);
      delete options.image;
      return self.registration.showNotification(title, options);
    })
  );
});

self.addEventListener('notificationclick', function (event) {
  console.log('[Service Worker] Notification click received:', event.notification);
  event.notification.close();

  const targetUrl = event.notification.data && event.notification.data.url ? event.notification.data.url : '/';

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true }).then(function (clientList) {
      for (let i = 0; i < clientList.length; i++) {
        const client = clientList[i];
        if (client.url.includes(self.location.origin) && 'focus' in client) {
          return client.focus();
        }
      }
      if (clients.openWindow) {
        return clients.openWindow(targetUrl);
      }
    })
  );
});
