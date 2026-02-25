using Land_Readjustment_Tool.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Numerics;
using System.Security.Policy;
using System.Xml;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Land_Readjustment_Tool.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _dbPath;

        public AppDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        // ── DB SETS ─────────────────────────────────
        public DbSet<ProjectInfo> ProjectInfo { get; set; }
        public DbSet<LandOwner> LandOwners { get; set; }
        public DbSet<MalpotReference> MalpotReferences { get; set; }
        public DbSet<ImportSession> ImportSessions { get; set; }
        public DbSet<ImportedRawRecord> ImportedRawRecords { get; set; }
        public DbSet<ValidationError> ValidationErrors { get; set; }
        public DbSet<CitizenshipConflict> CitizenshipConflicts { get; set; }
        public DbSet<CitizenshipConflictRecord> CitizenshipConflictRecords { get; set; }
        public DbSet<BaselineParcel> BaselineParcels { get; set; }
        public DbSet<CanvasLayer> CanvasLayers { get; set; }
        public DbSet<CanvasObject> CanvasObjects { get; set; }
        public DbSet<Road> Roads { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<ParcelFrontage> ParcelFrontages { get; set; }
        public DbSet<ContributionCategory> ContributionCategories { get; set; }
        public DbSet<ParcelContribution> ParcelContributions { get; set; }
        public DbSet<ParcelContributionSummary> ParcelContributionSummaries { get; set; }
        public DbSet<ReplottedParcel> ReplottedParcels { get; set; }
        public DbSet<OriginalToReplottedMap> OriginalToReplottedMaps { get; set; }
        public DbSet<ReplottedParcelOwner> ReplottedParcelOwners { get; set; }
        public DbSet<PlotType> PlotTypes { get; set; }

        // ── CONFIGURE CONNECTION ─────────────────────
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(
                $"Data Source={_dbPath}",
                x => x.UseNetTopologySuite());
        }

        // ── CONFIGURE RELATIONSHIPS ──────────────────
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            // ProjectInfo — always exactly one row
            modelBuilder.Entity<ProjectInfo>()
                .HasCheckConstraint("CK_ProjectInfo_SingleRow",
                    "Id = 1");

            // MalpotReference — unique per owner per moth
            modelBuilder.Entity<MalpotReference>()
                .HasIndex(m => new { m.LandOwnerId, m.MothNo })
                .IsUnique();

            // BaselineParcel — unique FullUniqueParcelCode
            modelBuilder.Entity<BaselineParcel>()
                .HasIndex(b => b.FullUniqueParcelCode)
                .IsUnique();

            // PlotType — unique TypeCode
            modelBuilder.Entity<PlotType>()
                .HasIndex(p => p.TypeCode)
                .IsUnique();

            // LandOwner — unique CitizenshipNumber
            // only when not null
            modelBuilder.Entity<LandOwner>()
                .HasIndex(l => l.CitizenshipNumber)
                .IsUnique()
                .HasFilter("CitizenshipNumber IS NOT NULL");

            // CitizenshipConflict — two FKs to same table
            modelBuilder.Entity<CitizenshipConflictRecord>()
                .HasOne(c => c.CitizenshipConflict)
                .WithMany(c => c.ConflictingRecords)
                .HasForeignKey(c => c.CitizenshipConflictId)
                .OnDelete(DeleteBehavior.Cascade);

            // ParcelFrontage — either baseline or replotted
            modelBuilder.Entity<ParcelFrontage>()
                .HasOne(f => f.BaselineParcel)
                .WithMany(b => b.ParcelFrontages)
                .HasForeignKey(f => f.BaselineParcelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParcelFrontage>()
                .HasOne(f => f.ReplottedParcel)
                .WithMany(r => r.ParcelFrontages)
                .HasForeignKey(f => f.ReplottedParcelId)
                .OnDelete(DeleteBehavior.Restrict);

            // OriginalToReplottedMap relationships
            modelBuilder.Entity<OriginalToReplottedMap>()
                .HasOne(o => o.BaselineParcel)
                .WithMany(b => b.OriginalToReplottedMaps)
                .HasForeignKey(o => o.BaselineParcelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OriginalToReplottedMap>()
                .HasOne(o => o.ReplottedParcel)
                .WithMany(r => r.OriginalToReplottedMaps)
                .HasForeignKey(o => o.ReplottedParcelId)
                .OnDelete(DeleteBehavior.Restrict);

            // BaselineParcel → CanvasObject (one to one)
            modelBuilder.Entity<BaselineParcel>()
                .HasOne(b => b.CanvasObject)
                .WithOne(c => c.BaselineParcel)
                .HasForeignKey<BaselineParcel>(b => b.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReplottedParcel → CanvasObject (one to one)
            modelBuilder.Entity<ReplottedParcel>()
                .HasOne(r => r.CanvasObject)
                .WithOne(c => c.ReplottedParcel)
                .HasForeignKey<ReplottedParcel>(r => r.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Road → CanvasObject (one to one)
            modelBuilder.Entity<Road>()
                .HasOne(r => r.CanvasObject)
                .WithOne(c => c.Road)
                .HasForeignKey<Road>(r => r.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Block → CanvasObject (one to one)
            modelBuilder.Entity<Block>()
                .HasOne(b => b.CanvasObject)
                .WithOne(c => c.Block)
                .HasForeignKey<Block>(b => b.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

        }

        // ── AUTO SET DATES ───────────────────────────
        public override int SaveChanges()
        {
            SetDates();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SetDates();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetDates()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added
                         || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Properties
                        .Any(p => p.Metadata.Name == "CreatedDate"))
                    {
                        entry.Property("CreatedDate").CurrentValue
                            = DateTime.Now;
                    }
                }

                if (entry.Properties
                    .Any(p => p.Metadata.Name == "LastModifiedDate"))
                {
                    entry.Property("LastModifiedDate").CurrentValue
                        = DateTime.Now;
                }
            }
        }
    }
}