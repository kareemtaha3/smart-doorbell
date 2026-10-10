export interface User {
  id: string;
  username: string;
  email: string;
  displayName: string;
  createdAt: string;
}

export interface HomeMember {
  id: string;
  userId: string;
  username: string;
  displayName: string;
  role: 'Owner' | 'Member';
  createdAt: string;
}

export interface Device {
  id: string;
  type: 'Button' | 'Camera' | 'Chime';
  status: 'Online' | 'Offline';
  lastSeenAt?: string;
}

export interface Home {
  id: string;
  name: string;
  ownerUserId: string;
  createdAt: string;
  members: HomeMember[];
  devices: Device[];
}

export interface Melody {
  id: string;
  homeId: string;
  name: string;
  toneFileUrl: string;
  isMain: boolean;
  createdAt: string;
}

export interface Invitation {
  id: string;
  homeId: string;
  homeName: string;
  invitedByUserId: string;
  invitedByUsername: string;
  invitedUserId: string;
  status: 'Pending' | 'Accepted' | 'Declined';
  createdAt: string;
}

export interface NotificationPayload {
  notificationId: string;
  sessionId: string;
  homeId: string;
  title: string;
  body: string;
  pressCount: number;
  imageUrl?: string | null;
  tag?: string;
  timestamp: string;
}

export interface NotificationDelivery {
  id: string;
  userId: string;
  channel: 'SignalR' | 'WebPush';
  sentAt: string;
  status: 'Sent' | 'Failed';
  readAt?: string | null;
}

export interface NotificationDetail {
  id: string;
  sessionId: string;
  homeId: string;
  title: string;
  body: string;
  pressCount: number;
  imageUrl?: string | null;
  createdAt: string;
  updatedAt: string;
  deliveries: NotificationDelivery[];
}

export interface RingSessionDetail {
  sessionId: string;
  buttonDeviceId: string;
  homeId: string;
  createdAt: string;
  lastPressAt: string;
  closedAt?: string | null;
  pressCount: number;
  escalatedAt?: string | null;
  notification?: NotificationDetail | null;
}

