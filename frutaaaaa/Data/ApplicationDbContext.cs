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
       // public DbSet<variete> varietes { get; set; }
        public DbSet<TPalette> TPalettes { get; set; }
        public DbSet<grpvar> GrpVars { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyProgramDetail>()
                 .HasKey(d => new { d.NumProg, d.codgrv });

            modelBuilder.Entity<Destination>(eb =>
            {
                eb.ToTable("destination");
                eb.HasNoKey();
                eb.Property(d => d.coddes).HasColumnName("coddes");
                eb.Property(d => d.vildes).HasColumnName("vildes");
            });
            modelBuilder.Entity<TPalette>(eb =>
            {
                eb.ToTable("tpalette");
                eb.HasNoKey();
                eb.Property(p => p.codtyp).HasColumnName("codtyp");
                eb.Property(p => p.nomemb).HasColumnName("nomemb");
            });

            modelBuilder.Entity<Partenaire>(eb =>
            {
                eb.ToTable("partenaire");
                eb.HasNoKey();
                eb.Property(p => p.@ref).HasColumnName("ref");
                eb.Property(p => p.nom).HasColumnName("nom");
                eb.Property(p => p.type).HasColumnName("type"); // Add this line
            });
            modelBuilder.Entity<grpvar>(eb =>
            {
                eb.ToTable("grpvar");
                eb.HasNoKey();
                eb.Property(g => g.codgrv).HasColumnName("codgrv");
                eb.Property(g => g.nomgrv).HasColumnName("nomgrv");
            });
        }
    }
}