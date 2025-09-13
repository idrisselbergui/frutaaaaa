using frutaaaaa.Models;
using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Trait> Traits { get; set; }
        public DbSet<Traitement> Traitements { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DailyProgram> DailyPrograms { get; set; }
        public DbSet<DailyProgramDetail> DailyProgramDetails { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Partenaire> Partenaires { get; set; }
        public DbSet<TPalette> TPalettes { get; set; }
        public DbSet<GrpVar> grpvars { get; set; }
        public DbSet<Verger> Vergers { get; set; }
        public DbSet<PalBrut> palbruts { get; set; }
        public DbSet<Palette> Palettes { get; set; }
        public DbSet<Palette_d> Palette_ds { get; set; }
        public DbSet<Variete> Varietes { get; set; }
        public DbSet<EcartE> EcartEs { get; set; }
        public DbSet<ViewExpVerVar> ViewExpVerVars { get; set; }
        public DbSet<Bdq> Bdqs { get; set; }
        public DbSet<Dossier> Dossiers { get; set; }
        public DbSet<TypeEcart> TypeEcarts { get; set; }
        public DbSet<EcartD> EcartDs { get; set; }
        public DbSet<Entreprise> Entreprises { get; set; }
        // --- NEW DbSet FOR RECEPTION ---
        public DbSet<Reception> Receptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing Configurations
            modelBuilder.Entity<Trait>().HasKey(t => t.Ref);
            modelBuilder.Entity<Traitement>().HasKey(t => t.Numtrait);


            modelBuilder.Entity<DailyProgramDetail>()
                .HasKey(d => new { d.NumProg, d.codgrv });

            modelBuilder.Entity<Destination>().ToTable("destination").HasNoKey();
            modelBuilder.Entity<Partenaire>().ToTable("partenaire").HasNoKey();
            modelBuilder.Entity<GrpVar>().ToTable("grpvar").HasNoKey();
            modelBuilder.Entity<TPalette>().ToTable("tpalette").HasNoKey();
            modelBuilder.Entity<Entreprise>(eb =>
            {
                eb.ToTable("entreprise");
                eb.HasKey(e => e.refent); // Set the primary key
            });
            modelBuilder.Entity<EcartD>(eb =>
            {
                eb.ToTable("ecart_d");
                eb.HasKey(e => e.numpre); // Set the primary key
            });
            modelBuilder.Entity<TypeEcart>(eb =>
            {
                eb.ToTable("typeecart");
                eb.HasKey(t => t.codtype); // Set the primary key
            });
            // --- NEW CONFIGURATION FOR RECEPTION ---
            modelBuilder.Entity<Reception>(eb =>
            {
                eb.ToTable("reception");
                eb.HasKey(r => r.Numrec);
            });
            // --- END NEW CONFIGURATION ---

            // --- THIS IS THE CORRECTED SECTION ---
            modelBuilder.Entity<Verger>(eb =>
            {
                eb.ToTable("verger");
                eb.HasNoKey();

            });

            modelBuilder.Entity<PalBrut>(eb =>
            {
                eb.ToTable("palbrut");
                eb.HasNoKey();
            });

            modelBuilder.Entity<Palette>(eb =>
            {
                eb.ToTable("palette");
                eb.HasNoKey();
            });

            modelBuilder.Entity<Palette_d>(eb =>
            {
                eb.ToTable("palette_d");
                eb.HasNoKey();
            });

            modelBuilder.Entity<Variete>(eb =>
            {
                eb.ToTable("variete");
                eb.HasNoKey();

            });
            modelBuilder.Entity<EcartE>(eb =>
            {
                eb.ToTable("ecart_e");
                eb.HasNoKey(); // As requested, this is a keyless entity for viewing data
            });
            modelBuilder.Entity<ViewExpVerVar>(eb =>
            {
                eb.ToView("view_expvervar"); // Indique à EF que c'est une vue
                eb.HasNoKey();
                // Mapper la colonne SUM
                eb.Property(v => v.pdscom).HasColumnName("SUM(p.pdscom)");
            });
            modelBuilder.Entity<Bdq>(eb =>
            {
                eb.ToTable("bdq");
                eb.HasNoKey();
            });

            modelBuilder.Entity<Dossier>(eb =>
            {
                eb.ToTable("dossier");
                eb.HasNoKey();
            });



        }
    }
}
