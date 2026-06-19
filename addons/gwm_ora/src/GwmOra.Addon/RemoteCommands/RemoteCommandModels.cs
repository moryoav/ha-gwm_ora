namespace GwmOra.Addon.RemoteCommands;

public sealed class RemoteCommandSnapshot
{
    public string Id { get; init; } = String.Empty;
    public string Vin { get; init; } = String.Empty;
    public string Name { get; init; } = String.Empty;
    public string State { get; init; } = "pending";
    public string Status { get; init; } = String.Empty;
    public string? SeqNo { get; init; }
    public string? ResultCode { get; init; }
    public string? ResultMessage { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class RemoteCommandUnavailableException : Exception
{
    public RemoteCommandUnavailableException(string message) : base(message)
    {
    }
}
