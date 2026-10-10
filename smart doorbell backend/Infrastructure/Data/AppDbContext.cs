using System.Reflection;
using Microsoft.EntityFrameworkCore;
using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Home> Homes => Set<Home>();
    public DbSet<HomeMember> HomeMembers => Set<HomeMember>();
    public DbSet<HomeInvitation> HomeInvitations => Set<HomeInvitation>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<RingSession> RingSessions => Set<RingSession>();
    public DbSet<RingEvent> RingEvents => Set<RingEvent>();
    public DbSet<CameraSnapshot> CameraSnapshots => Set<CameraSnapshot>();
    public DbSet<ChimeMelody> ChimeMelodies => Set<ChimeMelody>();
    public DbSet<ChimeCommand> ChimeCommands => Set<ChimeCommand>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
