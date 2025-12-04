using frutaaaaa.Models;
using Microsoft.EntityFrameworkCore;

namespace frutaaaaa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Defaut> Defauts { get; set; }

        // DbSets
        public DbSet<EcartDirect> EcartDirects { get; set; }
        // Add this to your DbContext class:
        public DbSet<UserPagePermission> UserPagePermissions { get; set; }

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
        public DbSet<Reception> Receptions { get; set; }
        public DbSet<Vente> Ventes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Trait entity with UTF-8 character set for special characters
            modelBuilder.Entity<Trait>(entity =>
            {
                entity.HasKey(t => t.Ref);

                // Configure string columns to use utf8mb4 character set
                entity.Property(t => t.Nomcom)
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(t => t.Matieractive)
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");

                entity.Property(t => t.Unite)
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_unicode_ci");
            });
            // In your DbContext OnModelCreating method
            modelBuilder.Entity<Defaut>(entity =>
            {
                entity.ToTable("defaut");
                entity.HasKey(e => e.Coddef);
                entity.Property(e => e.Coddef).HasColumnName("coddef");
                entity.Property(e => e.Intdef).HasColumnName("intdef");
                entity.Property(e => e.Famdef).HasColumnName("famdef");
            });

            modelBuilder.Entity<UserPagePermission>()
           .HasKey(x => new { x.UserId, x.PageName });

            //modelBuilder.Entity<EcartDirect>(eb =>
            //{
            //    eb.ToTable("ecart_direct");
            //    eb.HasKey(e => e.Numpal);
            //    eb.Property(e => e.Numpal).ValueGeneratedOnAdd();
            //    eb.Property(e => e.Pdsfru).HasColumnType("DOUBLE");

            //    // New relationship
            //    eb.HasOne(e => e.TypeEcart)
            //      .WithMany()
            //      .HasForeignKey(e => e.Codtype)
            //      .OnDelete(DeleteBehavior.SetNull);
            //});

            modelBuilder.Entity<EcartDirect>(eb =>
            {
                eb.ToTable("ecart_direct");
                eb.HasKey(e => e.Numpal);
                eb.Property(e => e.Numpal).ValueGeneratedOnAdd();
                eb.Property(e => e.Pdsfru).HasColumnType("DOUBLE");
                eb.Property(e => e.Numvent).HasColumnType("INT"); // Add this
                eb.Property(e => e.Pdsvent).HasColumnType("DOUBLE"); // Add this

                eb.HasOne(e => e.TypeEcart)
                  .WithMany()
                  .HasForeignKey(e => e.Codtype)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            // Update EcartE configuration in OnModelCreating:
            modelBuilder.Entity<EcartE>(eb =>
            {
                eb.ToTable("ecart_e");
                eb.HasKey(e => e.numpal);
                eb.Property(e => e.numvent).HasColumnType("INT"); // Add this
                eb.Property(e => e.pdsvent).HasColumnType("DOUBLE"); // Add this
            });

            // Add Vente configuration in OnModelCreating:
            modelBuilder.Entity<Vente>(eb =>
            {
                eb.ToTable("vente");
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Id).ValueGeneratedOnAdd();
                eb.Property(e => e.Date).HasColumnType("DATE");
                eb.Property(e => e.Price).HasColumnType("DOUBLE");
                eb.Property(e => e.PoidsTotal).HasColumnType("DOUBLE");
                eb.Property(e => e.MontantTotal).HasColumnType("DOUBLE");
                eb.Property(e => e.CreatedAt).HasColumnType("DATETIME");
            });

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
                eb.HasKey(e => e.refent);
            });

            modelBuilder.Entity<EcartD>(eb =>
            {
                eb.ToTable("ecart_d");
                eb.HasKey(e => e.numpre);
            });

            modelBuilder.Entity<TypeEcart>(eb =>
            {
                eb.ToTable("typeecart");
                eb.HasKey(t => t.codtype);
            });

            modelBuilder.Entity<Reception>(eb =>
            {
                eb.ToTable("reception");
                eb.HasKey(r => r.Numrec);
            });

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

            //modelBuilder.Entity<EcartE>(eb =>
            //{
            //    eb.ToTable("ecart_e");
            //    eb.HasNoKey();
            //});

            modelBuilder.Entity<ViewExpVerVar>(eb =>
            {
                eb.ToView("view_expvervar");
                eb.HasNoKey();
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