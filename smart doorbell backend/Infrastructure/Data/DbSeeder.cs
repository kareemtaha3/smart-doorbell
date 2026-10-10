using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        try
        {
            logger.LogInformation("[Database Seeder] Ensuring database schema exists...");
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("[Database Seeder] Database already seeded. Skipping initial seed.");
                return;
            }

            logger.LogInformation("[Database Seeder] Seeding initial test data for Smart Doorbell System...");

            var ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var familyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var homeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var buttonDeviceId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var cameraDeviceId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var chimeDeviceId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var melodyId = Guid.Parse("77777777-7777-7777-7777-777777777777");

            var ownerUser = new User
            {
                Id = ownerId,
                Username = "homeowner",
                Email = "owner@smartdoorbell.local",
                DisplayName = "Main Home Owner",
                PasswordHash = "Password123!",
                CreatedAt = DateTime.UtcNow
            };

            var familyUser = new User
            {
                Id = familyId,
                Username = "familymember",
                Email = "family@smartdoorbell.local",
                DisplayName = "Family Member",
                PasswordHash = "Password123!",
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(ownerUser, familyUser);

            var home = new Home
            {
                Id = homeId,
                Name = "Smart Sweet Home",
                OwnerUserId = ownerId,
                CreatedAt = DateTime.UtcNow
            };

            await context.Homes.AddAsync(home);

            var ownerMember = new HomeMember
            {
                Id = Guid.Parse("aa111111-1111-1111-1111-111111111111"),
                HomeId = homeId,
                UserId = ownerId,
                Role = HomeRole.Owner,
                CreatedAt = DateTime.UtcNow
            };

            var familyMember = new HomeMember
            {
                Id = Guid.Parse("aa222222-2222-2222-2222-222222222222"),
                HomeId = homeId,
                UserId = familyId,
                Role = HomeRole.Member,
                CreatedAt = DateTime.UtcNow
            };

            await context.HomeMembers.AddRangeAsync(ownerMember, familyMember);

            var buttonDevice = new Device
            {
                Id = buttonDeviceId,
                HomeId = homeId,
                Type = DeviceType.Button,
                AuthToken = "token-button-device-123",
                Status = DeviceStatus.Online,
                LastSeenAt = DateTime.UtcNow
            };

            var cameraDevice = new Device
            {
                Id = cameraDeviceId,
                HomeId = homeId,
                Type = DeviceType.Camera,
                AuthToken = "token-camera-device-456",
                Status = DeviceStatus.Online,
                LastSeenAt = DateTime.UtcNow
            };

            var chimeDevice = new Device
            {
                Id = chimeDeviceId,
                HomeId = homeId,
                Type = DeviceType.Chime,
                AuthToken = "token-chime-device-789",
                Status = DeviceStatus.Online,
                LastSeenAt = DateTime.UtcNow
            };

            await context.Devices.AddRangeAsync(buttonDevice, cameraDevice, chimeDevice);

            var mainMelody = new ChimeMelody
            {
                Id = melodyId,
                HomeId = homeId,
                Name = "Classic Westminster Chime",
                ToneFileUrl = "http://localhost:9000/doorbell-melodies/westminster.mp3",
                IsMain = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.ChimeMelodies.AddAsync(mainMelody);

            await context.SaveChangesAsync();

            logger.LogInformation("[Database Seeder] Initial test data successfully seeded into database!\n" +
                "===============================================================\n" +
                "  - Home Owner User ID : {OwnerId} (Username: homeowner, Pass: Password123!)\n" +
                "  - Family User ID     : {FamilyId} (Username: familymember, Pass: Password123!)\n" +
                "  - Home ID            : {HomeId}\n" +
                "  - Button Device ID   : {ButtonId}\n" +
                "  - Camera Device ID   : {CameraId} (AuthToken: token-camera-device-456)\n" +
                "  - Chime Device ID    : {ChimeId}\n" +
                "  - Main Melody ID     : {MelodyId}\n" +
                "===============================================================",
                ownerId, familyId, homeId, buttonDeviceId, cameraDeviceId, chimeDeviceId, melodyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Database Seeder] An error occurred while seeding the database.");
        }
    }
}

