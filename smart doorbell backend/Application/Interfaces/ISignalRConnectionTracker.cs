namespace smart_doorbell.Application.Interfaces;

public interface ISignalRConnectionTracker
{
    void AddConnection(Guid userId, string connectionId);
    void RemoveConnection(Guid userId, string connectionId);
    bool IsUserOnline(Guid userId);
    IReadOnlyCollection<string> GetUserConnections(Guid userId);
}
