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
        public DbSet<Marque> Marques { get; set; }
        public DbSet<MarqueAssignment> MarqueAssignments { get; set; }
        public DbSet<SampleTest> SampleTests { get; set; }
        public DbSet<DailyCheck> DailyChecks { get; set; }
        public DbSet<DailyCheckDetail> DailyCheckDetails { get; set; }
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

            modelBuilder.Entity<Marque>(eb =>
            {
                eb.ToTable("marque");
                eb.HasKey(m => m.codmar);
                eb.Property(m => m.codmar).HasColumnName("codmar");
                eb.Property(m => m.desmar).HasColumnName("desmar");
                eb.Property(m => m.lier).HasColumnName("lier");

                // Set collation to latin1_swedish_ci as per the CREATE TABLE
                eb.Property(m => m.desmar).UseCollation("latin1_swedish_ci");
                eb.Property(m => m.lier).UseCollation("latin1_swedish_ci");
            });

            modelBuilder.Entity<MarqueAssignment>(eb =>
            {
                eb.ToTable("marque_assignment");
                eb.HasKey(ma => ma.Id);
                eb.Property(ma => ma.Id).HasColumnName("id").ValueGeneratedOnAdd();
                eb.Property(ma => ma.Codmar).HasColumnName("codmar");
                eb.Property(ma => ma.Refver).HasColumnName("refver");
                eb.Property(ma => ma.Codvar).HasColumnName("codvar");

                // Unique index on (codmar, refver, codvar)
                eb.HasIndex(ma => new { ma.Codmar, ma.Refver, ma.Codvar }).IsUnique();
            });

            modelBuilder.Entity<SampleTest>(eb =>
            {
                eb.ToTable("sample_test");
                eb.HasKey(s => s.Id);
                eb.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
                eb.Property(s => s.Numrec).HasColumnName("numrec");
                eb.Property(s => s.Coddes).HasColumnName("coddes");
                eb.Property(s => s.Codvar).HasColumnName("codvar");
                eb.Property(s => s.StartDate).HasColumnName("start_date");
                eb.Property(s => s.InitialFruitCount).HasColumnName("initial_fruit_count");
                eb.Property(s => s.Status).HasColumnName("status").HasConversion<string>();
            });

            modelBuilder.Entity<DailyCheck>(eb =>
            {
                eb.ToTable("daily_check");
                eb.HasKey(d => d.Id);
                eb.Property(d => d.Id).HasColumnName("id").ValueGeneratedOnAdd();
                eb.Property(d => d.SampleTestId).HasColumnName("sample_test_id");
                eb.Property(d => d.CheckDate).HasColumnName("check_date");

                eb.HasOne(d => d.SampleTest)
                  .WithMany()
                  .HasForeignKey(d => d.SampleTestId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DailyCheckDetail>(eb =>
            {
                eb.ToTable("daily_check_detail");
                eb.HasKey(dd => dd.Id);
                eb.Property(dd => dd.Id).HasColumnName("id").ValueGeneratedOnAdd();
                eb.Property(dd => dd.DailyCheckId).HasColumnName("daily_check_id");
                eb.Property(dd => dd.DefectType).HasColumnName("defect_type").HasConversion<string>();
                eb.Property(dd => dd.Quantity).HasColumnName("quantity");

                eb.HasOne(dd => dd.DailyCheck)
                  .WithMany()
                  .HasForeignKey(dd => dd.DailyCheckId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
