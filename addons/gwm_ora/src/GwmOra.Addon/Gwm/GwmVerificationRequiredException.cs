namespace GwmOra.Addon.Gwm;

public sealed class GwmVerificationRequiredException : InvalidOperationException
{
    public GwmVerificationRequiredException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
