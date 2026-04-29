using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Contribution;
using Land_Readjustment_Tool.Core.Entities.Import;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Layout;
using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Core.Entities.Spatial;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

        // Project
        public DbSet<ProjectInfo> ProjectInfo { get; set; }
        public DbSet<ProjectSettings> ProjectSettings { get; set; }

        // Spatial master data
        public DbSet<CoordinateSystem> CoordinateSystems { get; set; }
        public DbSet<ProjectionParameters> ProjectionParameters { get; set; }
        public DbSet<DatumTransformation> DatumTransformations { get; set; }

        // Land data
        public DbSet<LandOwner> LandOwners { get; set; }
        public DbSet<MalpotReference> MalpotReferences { get; set; }
        public DbSet<BaselineParcel> BaselineParcels { get; set; }
        public DbSet<ParcelFrontage> ParcelFrontages { get; set; }

        // Import
        public DbSet<ImportSession> ImportSessions { get; set; }
        public DbSet<ImportedRawRecord> ImportedRawRecords { get; set; }
        public DbSet<ValidationError> ValidationErrors { get; set; }
        public DbSet<CitizenshipConflict> CitizenshipConflicts { get; set; }
        public DbSet<CitizenshipConflictRecord> CitizenshipConflictRecords { get; set; }

        // Canvas
        public DbSet<CanvasLayer> CanvasLayers { get; set; }
        public DbSet<CanvasObject> CanvasObjects { get; set; }

        // Layout
        public DbSet<Road> Roads { get; set; }
        public DbSet<Block> Blocks { get; set; }

        // Contribution
        public DbSet<ContributionCategory> ContributionCategories { get; set; }
        public DbSet<ParcelContribution> ParcelContributions { get; set; }
        public DbSet<ParcelContributionSummary> ParcelContributionSummaries { get; set; }

        // Replotting
        public DbSet<ReplottedParcel> ReplottedParcels { get; set; }
        public DbSet<OriginalToReplottedMap> OriginalToReplottedMaps { get; set; }
        public DbSet<ReplottedParcelOwner> ReplottedParcelOwners { get; set; }
        public DbSet<PlotType> PlotTypes { get; set; }

        // ── CONFIGURE CONNECTION ─────────────────────

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder builder = new()
            {
                DataSource = _dbPath
            };

            optionsBuilder.UseSqlite(
                builder.ToString(),
                x => x.UseNetTopologySuite());
        }

        // ── CONFIGURE RELATIONSHIPS ──────────────────

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            // ── SINGLE ROW CONSTRAINTS ───────────────

            modelBuilder.Entity<ProjectInfo>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_ProjectInfo_SingleRow", "Id = 1"));

            modelBuilder.Entity<ProjectSettings>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_ProjectSettings_SingleRow", "Id = 1"));

            // ── UNIQUE INDEXES ───────────────────────

            modelBuilder.Entity<CoordinateSystem>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<DatumTransformation>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<MalpotReference>()
                .HasIndex(m => new { m.LandOwnerId, m.MothNo })
                .IsUnique();

            modelBuilder.Entity<BaselineParcel>()
                .HasIndex(b => b.FullUniqueParcelCode)
                .IsUnique();

            modelBuilder.Entity<PlotType>()
                .HasIndex(p => p.TypeCode)
                .IsUnique();

            modelBuilder.Entity<LandOwner>()
                .HasIndex(l => l.CitizenshipNumber)
                .IsUnique()
                .HasFilter("CitizenshipNumber IS NOT NULL");

            modelBuilder.Entity<ImportedRawRecord>()
                .HasIndex(r => new { r.ImportSessionId, r.RowNumber })
                .IsUnique();

            modelBuilder.Entity<ValidationError>()
                .HasIndex(v => v.ImportedRawRecordId);

            // ── SPATIAL RELATIONSHIPS ────────────────

            // CoordinateSystem → ProjectionParameters (one to one)
            modelBuilder.Entity<CoordinateSystem>()
                .HasOne(c => c.ProjectionParameters)
                .WithOne(p => p.CoordinateSystem)
                .HasForeignKey<ProjectionParameters>(
                    p => p.CoordinateSystemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProjectSettings → CoordinateSystem
            modelBuilder.Entity<ProjectSettings>()
                .HasOne(s => s.CoordinateSystem)
                .WithMany()
                .HasForeignKey(s => s.CoordinateSystemId)
                .OnDelete(DeleteBehavior.SetNull);

            // ProjectSettings → DatumTransformation
            modelBuilder.Entity<ProjectSettings>()
                .HasOne(s => s.DatumTransformation)
                .WithMany()
                .HasForeignKey(s => s.DatumTransformationId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── LAND DATA RELATIONSHIPS ──────────────

            modelBuilder.Entity<CitizenshipConflictRecord>()
                .HasOne(c => c.CitizenshipConflict)
                .WithMany(c => c.ConflictingRecords)
                .HasForeignKey(c => c.CitizenshipConflictId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ImportedRawRecord>()
                .HasOne(r => r.ImportSession)
                .WithMany(s => s.ImportedRawRecords)
                .HasForeignKey(r => r.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ValidationError>()
                .HasOne(v => v.ImportSession)
                .WithMany(s => s.ValidationErrors)
                .HasForeignKey(v => v.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ValidationError>()
                .HasOne(v => v.ImportedRawRecord)
                .WithMany(r => r.ValidationErrors)
                .HasForeignKey(v => v.ImportedRawRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CitizenshipConflict>()
                .HasOne(c => c.ImportSession)
                .WithMany(s => s.CitizenshipConflicts)
                .HasForeignKey(c => c.ImportSessionId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // ── CANVAS OBJECT ONE-TO-ONE ─────────────

            modelBuilder.Entity<BaselineParcel>()
                .HasOne(b => b.CanvasObject)
                .WithOne(c => c.BaselineParcel)
                .HasForeignKey<BaselineParcel>(
                    b => b.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ReplottedParcel>()
                .HasOne(r => r.CanvasObject)
                .WithOne(c => c.ReplottedParcel)
                .HasForeignKey<ReplottedParcel>(
                    r => r.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Road>()
                .HasOne(r => r.CanvasObject)
                .WithOne(c => c.Road)
                .HasForeignKey<Road>(
                    r => r.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Block>()
                .HasOne(b => b.CanvasObject)
                .WithOne(c => c.Block)
                .HasForeignKey<Block>(
                    b => b.CanvasObjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── SEED DATA ────────────────────────────

            SeedPlotTypes(modelBuilder);
            SeedCoordinateSystems(modelBuilder);
            SeedProjectionParameters(modelBuilder);
            SeedDatumTransformations(modelBuilder);
        }

        // ── SEED METHODS ─────────────────────────────

        private static void SeedPlotTypes(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlotType>().HasData(
                new PlotType
                {
                    Id = 1,
                    TypeName = "Private",
                    TypeCode = "PRV",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    Description = "Private ownership plot"
                },
                new PlotType
                {
                    Id = 2,
                    TypeName = "Sales Plot",
                    TypeCode = "SAL",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 2,
                    Description = "Plot for sale to recover project costs"
                },
                new PlotType
                {
                    Id = 3,
                    TypeName = "Government",
                    TypeCode = "GOV",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 3,
                    Description = "Government use plot"
                },
                new PlotType
                {
                    Id = 4,
                    TypeName = "Open Space",
                    TypeCode = "OPS",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 4,
                    Description = "Parks and public open spaces"
                },
                new PlotType
                {
                    Id = 5,
                    TypeName = "Community",
                    TypeCode = "COM",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 5,
                    Description = "Community use plot"
                },
                new PlotType
                {
                    Id = 6,
                    TypeName = "Road",
                    TypeCode = "ROD",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 6,
                    Description = "Road right of way"
                }
            );
        }

        private static void SeedCoordinateSystems(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoordinateSystem>().HasData(
                new CoordinateSystem
                {
                    Id = 1,
                    Code = "UTM44N",
                    Name = "UTM Zone 44N — West Nepal",
                    EpsgCode = 32644,
                    ProjectionType = "TransverseMercator",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    Description = "West Nepal. 78°E to 84°E. WGS84 datum."
                },
                new CoordinateSystem
                {
                    Id = 2,
                    Code = "UTM45N",
                    Name = "UTM Zone 45N — East Nepal",
                    EpsgCode = 32645,
                    ProjectionType = "TransverseMercator",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 2,
                    Description = "East Nepal. 84°E to 90°E. WGS84 datum."
                },
                new CoordinateSystem
                {
                    Id = 3,
                    Code = "MUTM81",
                    Name = "Modified UTM Zone 81 — Nepal",
                    EpsgCode = null,
                    ProjectionType = "TransverseMercator",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 3,
                    Description = "Nepal Survey Dept. Central meridian 81°E. Everest 1830."
                },
                new CoordinateSystem
                {
                    Id = 4,
                    Code = "MUTM84",
                    Name = "Modified UTM Zone 84 — Nepal",
                    EpsgCode = null,
                    ProjectionType = "TransverseMercator",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 4,
                    Description = "Nepal Survey Dept. Central meridian 84°E. Everest 1830."
                },
                new CoordinateSystem
                {
                    Id = 5,
                    Code = "MUTM87",
                    Name = "Modified UTM Zone 87 — Nepal",
                    EpsgCode = null,
                    ProjectionType = "TransverseMercator",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 5,
                    Description = "Nepal Survey Dept. Central meridian 87°E. Everest 1830."
                }
                
            );
        }

        private static void SeedProjectionParameters(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectionParameters>().HasData(
                // MUTM81 — central meridian 81°E
                new ProjectionParameters
                {
                    Id = 1,
                    CoordinateSystemId = 3,
                    CentralMeridian = 81.0,
                    LatitudeOfOrigin = 0.0,
                    ScaleFactor = 0.9999,
                    FalseEasting = 500000.0,
                    FalseNorthing = 0.0,
                    Ellipsoid = "Everest1830",
                    SemiMajorAxis = 6377276.345,
                    InverseFlattening = 300.8017
                },
                // MUTM82 — central meridian 84°E
                new ProjectionParameters
                {
                    Id = 2,
                    CoordinateSystemId = 4,
                    CentralMeridian = 84.0,
                    LatitudeOfOrigin = 0.0,
                    ScaleFactor = 0.9999,
                    FalseEasting = 500000.0,
                    FalseNorthing = 0.0,
                    Ellipsoid = "Everest1830",
                    SemiMajorAxis = 6377276.345,
                    InverseFlattening = 300.8017
                },
                // MUTM83 — central meridian 87°E
                new ProjectionParameters
                {
                    Id = 3,
                    CoordinateSystemId = 5,
                    CentralMeridian = 87.0,
                    LatitudeOfOrigin = 0.0,
                    ScaleFactor = 0.9999,
                    FalseEasting = 500000.0,
                    FalseNorthing = 0.0,
                    Ellipsoid = "Everest1830",
                    SemiMajorAxis = 6377276.345,
                    InverseFlattening = 300.8017
                }
            );
        }

        private static void SeedDatumTransformations(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatumTransformation>().HasData(
                
                new DatumTransformation
                {
                    Id = 1,
                    Code = "NAGARKOT_TM",
                    Name = "Nagarkot TM",
                    SourceDatum = "Everest1830",
                    TargetDatum = "WGS84",
                    DeltaX = 296.207,
                    DeltaY = 731.545,
                    DeltaZ = 273.001,
                    RotationX = 0,
                    RotationY = 0,
                    RotationZ = 0,
                    ScalePpm = 0,
                    ApplicableCrsCodes = "MUTM81,MUTM82,MUTM83",
                    Source = "Nagarkot TM",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    Description = "Based on Nagarkot GPS control points."
                },
                new DatumTransformation
                {
                    Id = 2,
                    Code = "KALIANPUR",
                    Name = "Kalianpur Datum Parameters",
                    SourceDatum = "Everest1830",
                    TargetDatum = "WGS84",
                    DeltaX = 295.0,
                    DeltaY = 736.0,
                    DeltaZ = 257.0,
                    RotationX = 0,
                    RotationY = 0,
                    RotationZ = 0,
                    ScalePpm = 0,
                    ApplicableCrsCodes = "MUTM81,MUTM84,MUTM87",
                    Source = "Kalianpur datum parameters",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 2,
                    Description = "Traditional Kalianpur parameters. Used in older records."
                },
                new DatumTransformation
                {
                    Id = 3,
                    Code = "SURVEY_DEPT_7_PARAM",
                    Name = "Nepal Survey Department (Official)",
                    SourceDatum = "Everest1830",
                    TargetDatum = "WGS84",
                    DeltaX = -124.3813,
                    DeltaY = 521.67,
                    DeltaZ = 764.5137,
                    RotationX = 17.1488,
                    RotationY = -8.11536,
                    RotationZ = 11.1842,
                    ScalePpm = -2.1105,
                    ApplicableCrsCodes = "MUTM81,MUTM84,MUTM87",
                    Source = "Survey Department Nepal",
                    Region = "Nepal",
                    IsSystemDefault = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    Description = "Official transformation. Recommended for all MUTM zones."
                }
            );
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
