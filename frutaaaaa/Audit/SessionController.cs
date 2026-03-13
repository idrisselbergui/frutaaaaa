using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Audit
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionController : ControllerBase
    {
        private readonly AuditDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        // Thread-safe in-memory counter for failed login attempts, keyed by username
        private static readonly ConcurrentDictionary<string, int> _failedAttempts = new();

        public SessionController(AuditDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        // ═══════════════════════════════════════════════════════
        // ENDPOINT 1 — POST /api/sessions/failed
        // Record a failed login attempt (in-memory counter only)
        // ═══════════════════════════════════════════════════════
        [HttpPost("failed")]
        public IActionResult RecordFailedAttempt([FromBody] FailedAttemptRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username))
                    return BadRequest(new { message = "Username is required." });

                var count = _failedAttempts.AddOrUpdate(request.Username, 1, (_, current) => current + 1);
                return Ok(new { failedCount = count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Failed attempt error: {ex.Message}");
                return Ok(new { failedCount = 0 });
            }
        }

        // ═══════════════════════════════════════════════════════
        // ENDPOINT 2 — POST /api/sessions/start
        // Create a new session row on successful login
        // ═══════════════════════════════════════════════════════
        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username))
                    return BadRequest(new { message = "Username is required." });

                // Resolve IP server-side — NEVER trust the body
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                var ipAddress = remoteIp?.MapToIPv4().ToString() ?? "unknown";

                // Geolocation
                var (country, city) = await ResolveGeolocation(ipAddress);

                // Consume failed attempts counter
                var failedCount = 0;
                _failedAttempts.TryRemove(request.Username, out failedCount);

                var session = new UserSession
                {
                    UserId = request.UserId,
                    Username = request.Username,
                    TenantDatabase = request.TenantDatabase,
                    IpAddress = ipAddress,
                    Country = country,
                    City = city,
                    MachineName = request.MachineName,
                    Browser = request.Browser,
                    Os = request.Os,
                    LoginAt = DateTime.UtcNow,
                    LogoutAt = null,
                    SessionDuration = null,
                    Status = "ACTIVE",
                    FailedAttempts = failedCount,
                    CreatedAt = DateTime.UtcNow
                };

                _db.UserSessions.Add(session);
                await _db.SaveChangesAsync();

                return Ok(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Start session error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[SessionController] Inner: {ex.InnerException.Message}");
                return StatusCode(500, new { message = "Failed to create session." });
            }
        }

        // ═══════════════════════════════════════════════════════
        // ENDPOINT 3 — POST /api/sessions/page
        // Track page navigation during an active session
        // ═══════════════════════════════════════════════════════
        [HttpPost("page")]
        public async Task<IActionResult> TrackPage([FromBody] PageVisitRequest request)
        {
            try
            {
                if (request.SessionId <= 0)
                    return BadRequest(new { message = "Valid sessionId is required." });

                // Close the most recent open page visit for this session
                var openVisit = await _db.UserPageVisits
                    .Where(v => v.SessionId == request.SessionId && v.LeftAt == null)
                    .OrderByDescending(v => v.EnteredAt)
                    .FirstOrDefaultAsync();

                if (openVisit != null)
                {
                    openVisit.LeftAt = DateTime.UtcNow;
                    openVisit.TimeSpent = (int)(DateTime.UtcNow - openVisit.EnteredAt).TotalSeconds;
                }

                // Insert new page visit
                var visit = new UserPageVisit
                {
                    SessionId = request.SessionId,
                    UserId = request.UserId,
                    Username = request.Username,
                    PagePath = request.PagePath,
                    EnteredAt = DateTime.UtcNow,
                    LeftAt = null,
                    TimeSpent = null,
                    TenantDatabase = request.TenantDatabase
                };

                _db.UserPageVisits.Add(visit);
                await _db.SaveChangesAsync();

                return Ok(new { visitId = visit.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Page tracking error: {ex.Message}");
                return StatusCode(500, new { message = "Failed to track page visit." });
            }
        }

        // ═══════════════════════════════════════════════════════
        // ENDPOINT 4 — POST /api/sessions/end
        // Close a session on logout or tab close
        // ═══════════════════════════════════════════════════════
        [HttpPost("end")]
        public async Task<IActionResult> EndSession([FromBody] EndSessionRequest request)
        {
            try
            {
                if (request.SessionId <= 0)
                    return BadRequest(new { message = "Valid sessionId is required." });

                var session = await _db.UserSessions.FindAsync(request.SessionId);
                if (session == null)
                    return NotFound(new { message = "Session not found." });

                // Only close if still ACTIVE
                if (session.Status == "ACTIVE")
                {
                    session.LogoutAt = DateTime.UtcNow;
                    session.SessionDuration = (int)(DateTime.UtcNow - session.LoginAt).TotalSeconds;
                    session.Status = request.Status ?? "LOGGED_OUT";
                }

                // Close any open page visit for this session
                var openVisit = await _db.UserPageVisits
                    .Where(v => v.SessionId == request.SessionId && v.LeftAt == null)
                    .OrderByDescending(v => v.EnteredAt)
                    .FirstOrDefaultAsync();

                if (openVisit != null)
                {
                    openVisit.LeftAt = DateTime.UtcNow;
                    openVisit.TimeSpent = (int)(DateTime.UtcNow - openVisit.EnteredAt).TotalSeconds;
                }

                await _db.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] End session error: {ex.Message}");
                return StatusCode(500, new { message = "Failed to end session." });
            }
        }

        // ═══════════════════════════════════════════════════════
        // PRIVATE — IP Geolocation via ip-api.com
        // ═══════════════════════════════════════════════════════
        private async Task<(string country, string city)> ResolveGeolocation(string ipAddress)
        {
            try
            {
                // Check for loopback and private IPs — skip HTTP call entirely
                if (IsPrivateOrLoopback(ipAddress))
                    return ("Local", "Local");

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.GetAsync($"http://ip-api.com/json/{ipAddress}?fields=status,country,city");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);

                    if (data.TryGetProperty("status", out var statusProp) &&
                        statusProp.GetString() == "success")
                    {
                        var country = data.TryGetProperty("country", out var c) ? c.GetString() ?? "Unknown" : "Unknown";
                        var city = data.TryGetProperty("city", out var ci) ? ci.GetString() ?? "Unknown" : "Unknown";
                        return (country, city);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Geolocation failed: {ex.Message}");
            }

            return ("Local", "Local");
        }

        /// <summary>
        /// Check if an IP address is loopback or in a private RFC 1918 range.
        /// </summary>
        private static bool IsPrivateOrLoopback(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return true;

            if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress == "0.0.0.0")
                return true;

            if (IPAddress.TryParse(ipAddress, out var ip))
            {
                if (IPAddress.IsLoopback(ip))
                    return true;

                var bytes = ip.MapToIPv4().GetAddressBytes();
                // 10.x.x.x
                if (bytes[0] == 10) return true;
                // 172.16.x.x – 172.31.x.x
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
                // 192.168.x.x
                if (bytes[0] == 192 && bytes[1] == 168) return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════
        // REQUEST DTOs
        // ═══════════════════════════════════════════════════════

        public class FailedAttemptRequest
        {
            public string? Username { get; set; }
            public string? TenantDatabase { get; set; }
        }

        public class StartSessionRequest
        {
            public int? UserId { get; set; }
            public string? Username { get; set; }
            public string? TenantDatabase { get; set; }
            public string? MachineName { get; set; }
            public string? Browser { get; set; }
            public string? Os { get; set; }
        }

        public class PageVisitRequest
        {
            public long SessionId { get; set; }
            public int? UserId { get; set; }
            public string? Username { get; set; }
            public string? PagePath { get; set; }
            public string? TenantDatabase { get; set; }
        }

        public class EndSessionRequest
        {
            public long SessionId { get; set; }
            public string? Status { get; set; } // LOGGED_OUT, TAB_CLOSED, EXPIRED
        }
    }
}
