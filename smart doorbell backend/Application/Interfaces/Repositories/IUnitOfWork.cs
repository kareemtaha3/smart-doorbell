namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IHomeRepository Homes { get; }
    IHomeMemberRepository HomeMembers { get; }
    IHomeInvitationRepository HomeInvitations { get; }
    IPushSubscriptionRepository PushSubscriptions { get; }
    INotificationRepository Notifications { get; }
    INotificationDeliveryRepository NotificationDeliveries { get; }
    IRingSessionRepository RingSessions { get; }
    IDeviceRepository Devices { get; }
    ICameraSnapshotRepository CameraSnapshots { get; }
    IChimeMelodyRepository ChimeMelodies { get; }
    IChimeCommandRepository ChimeCommands { get; }
    IRingEventRepository RingEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
