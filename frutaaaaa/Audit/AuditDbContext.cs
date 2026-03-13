using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Audit
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserPageVisit> UserPageVisits { get; set; }

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

            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("user_sessions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Status).HasColumnType("VARCHAR(20)");
                entity.Property(e => e.LoginAt).HasColumnType("DATETIME");
                entity.Property(e => e.LogoutAt).HasColumnType("DATETIME");
                entity.Property(e => e.CreatedAt).HasColumnType("DATETIME");
            });

            modelBuilder.Entity<UserPageVisit>(entity =>
            {
                entity.ToTable("user_page_visits");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.EnteredAt).HasColumnType("DATETIME");
                entity.Property(e => e.LeftAt).HasColumnType("DATETIME");

                entity.HasOne(e => e.Session)
                    .WithMany()
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

