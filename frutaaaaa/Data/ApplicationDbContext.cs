using frutaaaaa.Models;
using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<DailyProgram> DailyPrograms { get; set; }
        public DbSet<DailyProgramDetail> DailyProgramDetails { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Partenaire> Partenaires { get; set; }
        public DbSet<variete> varietes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyProgramDetail>()
                .HasKey(d => new { d.NumProg, d.Codvar });

            modelBuilder.Entity<Destination>(eb =>
            {
                eb.ToTable("destination");
                eb.HasNoKey();
                eb.Property(d => d.coddes).HasColumnName("coddes");
                eb.Property(d => d.vildes).HasColumnName("vildes");
            });

            modelBuilder.Entity<Partenaire>(eb =>
            {
                eb.ToTable("partenaire");
                eb.HasNoKey();
                eb.Property(p => p.@ref).HasColumnName("ref");
                eb.Property(p => p.nom).HasColumnName("nom");
            });
            modelBuilder.Entity<variete>(eb =>
            {
                eb.ToTable("variete");
                eb.HasNoKey();
                eb.Property(p => p.codvar).HasColumnName("codvar");
                eb.Property(p => p.nomvar).HasColumnName("nomvar");
            });
        }
    }
}