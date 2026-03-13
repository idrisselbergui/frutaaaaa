using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Audit
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ActionType).HasColumnType("VARCHAR(10)");
                entity.Property(e => e.OldValues).HasColumnType("LONGTEXT");
                entity.Property(e => e.NewValues).HasColumnType("LONGTEXT");
                entity.Property(e => e.CreatedAt).HasColumnType("DATETIME");
            });
        }
    }
}
