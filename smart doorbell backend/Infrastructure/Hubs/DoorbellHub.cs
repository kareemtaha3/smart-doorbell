using Microsoft.AspNetCore.SignalR;
using smart_doorbell.Application.Interfaces;

namespace smart_doorbell.Infrastructure.Hubs;

public class DoorbellHub : Hub
{
    private readonly ISignalRConnectionTracker _connectionTracker;

    public DoorbellHub(ISignalRConnectionTracker connectionTracker)
    {
        _connectionTracker = connectionTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdStr = Context.UserIdentifier ?? Context.GetHttpContext()?.Request.Query["userId"];
        if (Guid.TryParse(userIdStr, out var userId))
        {
            _connectionTracker.AddConnection(userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdStr = Context.UserIdentifier ?? Context.GetHttpContext()?.Request.Query["userId"];
        if (Guid.TryParse(userIdStr, out var userId))
        {
            _connectionTracker.RemoveConnection(userId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinHomeGroup(string homeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Home_{homeId}");
    }

    public async Task LeaveHomeGroup(string homeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Home_{homeId}");
    }
}
