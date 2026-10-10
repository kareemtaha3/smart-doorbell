using System.Collections.Concurrent;
using smart_doorbell.Application.Interfaces;

namespace smart_doorbell.Infrastructure.Notifications;

public class SignalRConnectionTracker : ISignalRConnectionTracker
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        _userConnections.AddOrUpdate(
            userId,
            _ => new HashSet<string> { connectionId },
            (_, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                }
                return connections;
            });
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }
    }

    public bool IsUserOnline(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.Count > 0;
            }
        }
        return false;
    }

    public IReadOnlyCollection<string> GetUserConnections(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList().AsReadOnly();
            }
        }
        return Array.Empty<string>();
    }
}
