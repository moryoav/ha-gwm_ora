using System.Collections.Concurrent;

namespace GwmOra.Addon.RemoteCommands;

public sealed class RemoteCommandStore
{
    private const int MaxStatusLength = 240;
    private readonly ConcurrentDictionary<string, RemoteCommandSnapshot> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _lastStatusByVin = new(StringComparer.OrdinalIgnoreCase);

    public RemoteCommandSnapshot Create(string vin, string name)
    {
        var command = new RemoteCommandSnapshot
        {
            Id = Guid.NewGuid().ToString("N"),
            Vin = vin,
            Name = name,
            State = "pending",
            Status = Normalize($"{name}: queued"),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _commands[command.Id] = command;
        _lastStatusByVin[vin] = command.Status;
        return command;
    }

    public RemoteCommandSnapshot? Get(string id)
    {
        return _commands.TryGetValue(id, out var command) ? command : null;
    }

    public string GetLastStatus(string vin)
    {
        return _lastStatusByVin.TryGetValue(vin, out var status)
            ? status
            : "No remote command has run yet";
    }

    public RemoteCommandSnapshot Update(
        string id,
        string state,
        string status,
        string? seqNo = null,
        string? resultCode = null,
        string? resultMessage = null)
    {
        return _commands.AddOrUpdate(
            id,
            _ => throw new InvalidOperationException($"Remote command not found: {id}"),
            (_, existing) =>
            {
                var normalized = Normalize(status);
                var next = new RemoteCommandSnapshot
                {
                    Id = existing.Id,
                    Vin = existing.Vin,
                    Name = existing.Name,
                    State = state,
                    Status = normalized,
                    SeqNo = seqNo ?? existing.SeqNo,
                    ResultCode = resultCode ?? existing.ResultCode,
                    ResultMessage = resultMessage ?? existing.ResultMessage,
                    CreatedAt = existing.CreatedAt,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _lastStatusByVin[next.Vin] = next.Status;
                return next;
            });
    }

    private static string Normalize(string status)
    {
        status = String.Join(" ", (status ?? String.Empty).Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries));
        return status.Length <= MaxStatusLength
            ? status
            : status[..(MaxStatusLength - 3)] + "...";
    }
}
