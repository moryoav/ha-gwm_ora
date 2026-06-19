using System.Net;

namespace GwmOra.Addon.Ingress;

public static class IngressAccess
{
    private static readonly IPAddress SupervisorIngressProxy = IPAddress.Parse("172.30.32.2");

    public static bool IsAllowed(HttpContext context)
    {
        if (String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN")))
        {
            return true;
        }

        return Equals(context.Connection.RemoteIpAddress?.MapToIPv4(), SupervisorIngressProxy);
    }
}
