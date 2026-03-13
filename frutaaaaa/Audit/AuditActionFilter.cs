using Microsoft.AspNetCore.Mvc.Filters;

namespace frutaaaaa.Audit
{
    /// <summary>
    /// Layer 1: Global action filter that extracts HTTP-level audit data
    /// (who, where, when) and stores it in the scoped AuditContext.
    /// Skips GET requests entirely.
    /// </summary>
    public class AuditActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            // Skip GET requests — no audit for reads
            if (string.Equals(httpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            // Resolve the scoped AuditContext from DI
            var auditContext = httpContext.RequestServices.GetService<AuditContext>();
            if (auditContext == null) return;

            // --- User identity from custom headers ---
            var userIdHeader = httpContext.Request.Headers["X-User-Id"].FirstOrDefault();
            auditContext.UserId = int.TryParse(userIdHeader, out var uid) ? uid : null;

            var usernameHeader = httpContext.Request.Headers["X-Username"].FirstOrDefault();
            auditContext.Username = string.IsNullOrEmpty(usernameHeader) ? null : usernameHeader;

            // --- IP Address ---
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            auditContext.IpAddress = remoteIp?.MapToIPv4().ToString();

            // --- Machine Name from custom header ---
            var machineHeader = httpContext.Request.Headers["X-Machine-Name"].FirstOrDefault();
            auditContext.MachineName = string.IsNullOrEmpty(machineHeader) ? null : machineHeader;

            // --- Browser and OS parsed from User-Agent ---
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "";
            auditContext.Browser = ParseBrowser(userAgent);
            auditContext.Os = ParseOs(userAgent);

            // --- Request info ---
            auditContext.Endpoint = httpContext.Request.Path.ToString();
            auditContext.HttpMethod = httpContext.Request.Method;

            // --- Tenant database (metadata only — not used for routing) ---
            var dbHeader = httpContext.Request.Headers["X-Database-Name"].FirstOrDefault();
            auditContext.TenantDatabase = string.IsNullOrEmpty(dbHeader) ? null : dbHeader;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No-op — all work is done in OnActionExecuting
        }

        /// <summary>
        /// Simple User-Agent browser detection.
        /// </summary>
        private static string? ParseBrowser(string ua)
        {
            if (string.IsNullOrEmpty(ua)) return null;

            if (ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Edge";
            if (ua.Contains("OPR/", StringComparison.OrdinalIgnoreCase) || ua.Contains("Opera", StringComparison.OrdinalIgnoreCase)) return "Opera";
            if (ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Chrome";
            if (ua.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) return "Firefox";
            if (ua.Contains("Safari/", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Safari";

            return "Other";
        }

        /// <summary>
        /// Simple User-Agent OS detection.
        /// </summary>
        private static string? ParseOs(string ua)
        {
            if (string.IsNullOrEmpty(ua)) return null;

            if (ua.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows";
            if (ua.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) || ua.Contains("Macintosh", StringComparison.OrdinalIgnoreCase)) return "macOS";
            if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux";
            if (ua.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android";
            if (ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iOS";

            return "Other";
        }
    }
}
