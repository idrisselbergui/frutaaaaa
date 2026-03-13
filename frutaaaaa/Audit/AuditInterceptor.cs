using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace frutaaaaa.Audit
{
    /// <summary>
    /// Layer 2: EF Core SaveChanges interceptor that detects entity changes
    /// (INSERT/UPDATE/DELETE) and writes audit records to fruta_web_journal.
    /// 
    /// Fail-safe: if the audit write fails, the business operation is NOT affected.
    /// </summary>
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly AuditContext _auditContext;
        private readonly IConfiguration _configuration;

        // Temporary storage for pre-save snapshots
        private List<AuditEntry>? _pendingEntries;

        public AuditInterceptor(AuditContext auditContext, IConfiguration configuration)
        {
            _auditContext = auditContext;
            _configuration = configuration;
        }

        /// <summary>
        /// BEFORE save: capture old values and entity states.
        /// This MUST happen before save because EF clears change tracking post-save.
        /// </summary>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            try
            {
                var entries = eventData.Context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added
                             || e.State == EntityState.Modified
                             || e.State == EntityState.Deleted)
                    .ToList();

                if (entries.Count == 0)
                {
                    _pendingEntries = null;
                    return base.SavingChangesAsync(eventData, result, cancellationToken);
                }

                _pendingEntries = new List<AuditEntry>();

                foreach (var entry in entries)
                {
                    var auditEntry = new AuditEntry
                    {
                        EntityName = entry.Entity.GetType().Name,
                        ActionType = entry.State switch
                        {
                            EntityState.Added => "INSERT",
                            EntityState.Modified => "UPDATE",
                            EntityState.Deleted => "DELETE",
                            _ => "UNKNOWN"
                        },
                        EntityState = entry.State
                    };

                    // Capture primary key (may be 0 for Added if auto-generated)
                    var primaryKey = entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .Select(p => p.CurrentValue)
                        .FirstOrDefault();
                    auditEntry.EntityId = primaryKey?.ToString();

                    // Capture OLD values (before save)
                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        var oldValues = new Dictionary<string, object?>();
                        foreach (var prop in entry.Properties)
                        {
                            oldValues[prop.Metadata.Name] = entry.State == EntityState.Modified
                                ? prop.OriginalValue
                                : prop.CurrentValue; // For Deleted, current IS the old value
                        }
                        auditEntry.OldValues = oldValues;
                    }

                    // For Added/Modified, capture a reference to grab new values after save
                    auditEntry.Entry = entry;

                    _pendingEntries.Add(auditEntry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditInterceptor] Error capturing pre-save state: {ex.Message}");
                _pendingEntries = null;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// AFTER save: capture new values (including auto-generated IDs) and write to journal.
        /// </summary>
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (_pendingEntries != null && _pendingEntries.Count > 0)
            {
                try
                {
                    var auditLogs = new List<AuditLog>();
                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    };

                    foreach (var pending in _pendingEntries)
                    {
                        // Capture NEW values (after save — auto-generated IDs are now available)
                        Dictionary<string, object?>? newValues = null;
                        if (pending.EntityState == EntityState.Added || pending.EntityState == EntityState.Modified)
                        {
                            if (pending.Entry != null)
                            {
                                newValues = new Dictionary<string, object?>();
                                foreach (var prop in pending.Entry.Properties)
                                {
                                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                                }

                                // For Added entities, update the entity ID now that it's generated
                                if (pending.EntityState == EntityState.Added)
                                {
                                    var pk = pending.Entry.Properties
                                        .Where(p => p.Metadata.IsPrimaryKey())
                                        .Select(p => p.CurrentValue)
                                        .FirstOrDefault();
                                    pending.EntityId = pk?.ToString();
                                }
                            }
                        }

                        auditLogs.Add(new AuditLog
                        {
                            UserId = _auditContext.UserId,
                            Username = _auditContext.Username,
                            IpAddress = _auditContext.IpAddress,
                            MachineName = _auditContext.MachineName,
                            Browser = _auditContext.Browser,
                            Os = _auditContext.Os,
                            ActionType = pending.ActionType,
                            EntityName = pending.EntityName,
                            EntityId = pending.EntityId,
                            Endpoint = _auditContext.Endpoint,
                            HttpMethod = _auditContext.HttpMethod,
                            OldValues = pending.OldValues != null
                                ? JsonSerializer.Serialize(pending.OldValues, jsonOptions)
                                : null,
                            NewValues = newValues != null
                                ? JsonSerializer.Serialize(newValues, jsonOptions)
                                : null,
                            TenantDatabase = _auditContext.TenantDatabase,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Write to fruta_web_journal using a dedicated AuditDbContext
                    await WriteAuditLogsAsync(auditLogs, cancellationToken);
                }
                catch (Exception ex)
                {
                    // FAIL-SAFE: never break the business operation
                    Console.WriteLine($"[AuditInterceptor] Failed to write audit log: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"[AuditInterceptor] Inner: {ex.InnerException.Message}");
                }
                finally
                {
                    _pendingEntries = null;
                }
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Creates a dedicated AuditDbContext and writes audit records.
        /// </summary>
        private async Task WriteAuditLogsAsync(List<AuditLog> logs, CancellationToken ct)
        {
            var journalConnStr = _configuration.GetConnectionString("JournalConnection");
            if (string.IsNullOrEmpty(journalConnStr))
            {
                Console.WriteLine("[AuditInterceptor] JournalConnection not configured — skipping audit write.");
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
            optionsBuilder.UseMySql(journalConnStr, ServerVersion.AutoDetect(journalConnStr));

            using var auditDb = new AuditDbContext(optionsBuilder.Options);
            auditDb.AuditLogs.AddRange(logs);
            await auditDb.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Internal data class for tracking pre-save snapshots.
        /// </summary>
        private class AuditEntry
        {
            public string EntityName { get; set; } = null!;
            public string ActionType { get; set; } = null!;
            public string? EntityId { get; set; }
            public EntityState EntityState { get; set; }
            public Dictionary<string, object?>? OldValues { get; set; }
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? Entry { get; set; }
        }
    }
}
