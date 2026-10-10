using Microsoft.EntityFrameworkCore;
using Minio;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Application.Services;
using smart_doorbell.Infrastructure.Data;
using smart_doorbell.Infrastructure.Hubs;
using smart_doorbell.Infrastructure.Notifications;
using smart_doorbell.Infrastructure.Repositories;
using smart_doorbell.Infrastructure.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis Registration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IRedisSessionCache, RedisSessionCache>();

// External & Infrastructure Services
builder.Services.AddSingleton<ISignalRConnectionTracker, SignalRConnectionTracker>();
builder.Services.AddTransient<IWebPushService, WebPushService>();
builder.Services.AddSingleton<MqttClientService>();
builder.Services.AddSingleton<IMqttClientService>(sp => sp.GetRequiredService<MqttClientService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttClientService>());

// MinIO Registration
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["MinioOptions:Endpoint"] ?? "localhost:9000";
    var accessKey = config["MinioOptions:AccessKey"] ?? "minioadmin";
    var secretKey = config["MinioOptions:SecretKey"] ?? "minioadmin";

    bool useSsl = endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    endpoint = endpoint.Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                       .Replace("https://", "", StringComparison.OrdinalIgnoreCase);

    var clientBuilder = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey);

    if (useSsl)
    {
        clientBuilder = clientBuilder.WithSSL();
    }

    return clientBuilder.Build();
});

builder.Services.AddSingleton<IMinioStorageService, MinioStorageService>();

// Repositories & Unit of Work (Infrastructure Layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IHomeMemberRepository, HomeMemberRepository>();
builder.Services.AddScoped<IHomeInvitationRepository, HomeInvitationRepository>();
builder.Services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
builder.Services.AddScoped<IRingSessionRepository, RingSessionRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<ICameraSnapshotRepository, CameraSnapshotRepository>();
builder.Services.AddScoped<IChimeMelodyRepository, ChimeMelodyRepository>();
builder.Services.AddScoped<IChimeCommandRepository, ChimeCommandRepository>();
builder.Services.AddScoped<IRingEventRepository, RingEventRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services (Application Layer)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IPushSubscriptionService, PushSubscriptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDoorbellService, DoorbellService>();
builder.Services.AddScoped<IMelodyService, MelodyService>();
builder.Services.AddScoped<ICameraBrokerService, CameraBrokerService>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

var app = builder.Build();

// Seed Database with test data on startup
await DbSeeder.SeedAsync(app.Services);

// Ensure MinIO Buckets exist on startup
using (var scope = app.Services.CreateScope())
{
    var minioService = scope.ServiceProvider.GetRequiredService<IMinioStorageService>();
    var snapshotsBucket = builder.Configuration["MinioOptions:BucketName"] ?? "doorbell-snapshots";
    await minioService.EnsureBucketExistsAsync(snapshotsBucket);
    await minioService.EnsureBucketExistsAsync("doorbell-melodies");
}

// Swagger UI: http://localhost:5147/swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Doorbell API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DoorbellHub>("/hubs/doorbell");

app.MapGet("/health/database", async (AppDbContext db) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync();
        return Results.Ok(new
        {
            connected,
            database = "DoorBellDB",
            message = connected ? "SQL Server connection successful." : "SQL Server connection failed."
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection error: {ex.Message}");
    }
});

app.Run();
