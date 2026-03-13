namespace frutaaaaa.Audit
{
    /// <summary>
    /// Scoped bridge service. Populated by AuditActionFilter (Layer 1, HTTP context)
    /// and read by AuditInterceptor (Layer 2, EF Core context).
    /// </summary>
    public class AuditContext
    {
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? IpAddress { get; set; }
        public string? MachineName { get; set; }
        public string? Browser { get; set; }
        public string? Os { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public string? TenantDatabase { get; set; }
    }
}
