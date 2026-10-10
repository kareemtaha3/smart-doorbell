using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private bool _disposed;

    public IUserRepository Users { get; }
    public IHomeRepository Homes { get; }
    public IHomeMemberRepository HomeMembers { get; }
    public IHomeInvitationRepository HomeInvitations { get; }
    public IPushSubscriptionRepository PushSubscriptions { get; }
    public INotificationRepository Notifications { get; }
    public INotificationDeliveryRepository NotificationDeliveries { get; }
    public IRingSessionRepository RingSessions { get; }
    public IDeviceRepository Devices { get; }
    public ICameraSnapshotRepository CameraSnapshots { get; }
    public IChimeMelodyRepository ChimeMelodies { get; }
    public IChimeCommandRepository ChimeCommands { get; }
    public IRingEventRepository RingEvents { get; }

    public UnitOfWork(
        AppDbContext db,
        IUserRepository users,
        IHomeRepository homes,
        IHomeMemberRepository homeMembers,
        IHomeInvitationRepository homeInvitations,
        IPushSubscriptionRepository pushSubscriptions,
        INotificationRepository notifications,
        INotificationDeliveryRepository notificationDeliveries,
        IRingSessionRepository ringSessions,
        IDeviceRepository devices,
        ICameraSnapshotRepository cameraSnapshots,
        IChimeMelodyRepository chimeMelodies,
        IChimeCommandRepository chimeCommands,
        IRingEventRepository ringEvents)
    {
        _db = db;
        Users = users;
        Homes = homes;
        HomeMembers = homeMembers;
        HomeInvitations = homeInvitations;
        PushSubscriptions = pushSubscriptions;
        Notifications = notifications;
        NotificationDeliveries = notificationDeliveries;
        RingSessions = ringSessions;
        Devices = devices;
        CameraSnapshots = cameraSnapshots;
        ChimeMelodies = chimeMelodies;
        ChimeCommands = chimeCommands;
        RingEvents = ringEvents;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            _disposed = true;
        }
    }
}
