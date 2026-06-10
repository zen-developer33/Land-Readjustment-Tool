using Land_Pooling_Policy_Manager.Entities.Policy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Land_Pooling_Policy_Manager.Data
{
    /// <summary>
    /// Self-contained SQLite DbContext owned by the policy manager. Maps to the
    /// same tblPolicy* tables the main application uses, so when this manager
    /// connects to a project's .lpp file the data is shared. When standalone it
    /// connects to %AppData%\RePlot\PolicyManager\policies.db (see Program.cs).
    /// </summary>
    public sealed class PolicyDbContext : DbContext
    {
        private readonly string _dbPath;

        public PolicyDbContext(string dbPath)
        {
            _dbPath = dbPath ?? throw new ArgumentNullException(nameof(dbPath));
        }

        public DbSet<PolicySet> PolicySets => Set<PolicySet>();
        public DbSet<PolicySectionDefinition> PolicySectionDefinitions => Set<PolicySectionDefinition>();
        public DbSet<PolicyClause> PolicyClauses => Set<PolicyClause>();
        public DbSet<PolicyParameter> PolicyParameters => Set<PolicyParameter>();
        public DbSet<PolicyLookupTable> PolicyLookupTables => Set<PolicyLookupTable>();
        public DbSet<PolicyLookupColumn> PolicyLookupColumns => Set<PolicyLookupColumn>();
        public DbSet<PolicyLookupRow> PolicyLookupRows => Set<PolicyLookupRow>();
        public DbSet<PolicyLookupCell> PolicyLookupCells => Set<PolicyLookupCell>();
        public DbSet<PolicyAttachment> PolicyAttachments => Set<PolicyAttachment>();
        public DbSet<PolicyAuditEntry> PolicyAuditEntries => Set<PolicyAuditEntry>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SqliteConnectionStringBuilder builder = new() { DataSource = _dbPath };
            optionsBuilder.UseSqlite(builder.ToString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PolicySet>()
                .HasIndex(p => new { p.PolicyGroupKey, p.VersionNo })
                .IsUnique();

            modelBuilder.Entity<PolicySet>()
                .HasIndex(p => p.PolicyCode);

            modelBuilder.Entity<PolicyClause>()
                .HasIndex(c => new { c.PolicySetId, c.ClauseCode });

            modelBuilder.Entity<PolicyParameter>()
                .HasIndex(p => new { p.PolicySetId, p.ParameterKey });

            modelBuilder.Entity<PolicyLookupTable>()
                .HasIndex(t => new { t.PolicySetId, t.TableKey })
                .IsUnique();

            modelBuilder.Entity<PolicyLookupTable>()
                .HasIndex(t => t.PolicyClauseId);

            modelBuilder.Entity<PolicySectionDefinition>()
                .HasIndex(s => new { s.PolicySetId, s.SectionCode })
                .IsUnique();

            modelBuilder.Entity<PolicySectionDefinition>()
                .HasOne(s => s.PolicySet)
                .WithMany(p => p.Sections)
                .HasForeignKey(s => s.PolicySetId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>
        /// CREATE TABLE IF NOT EXISTS for every policy-manager table, so an
        /// .lpp file that pre-dates the policy manager (or a brand-new
        /// standalone policies.db) gets the schema without needing migrations.
        /// </summary>
        public async Task EnsureSchemaAsync(CancellationToken ct = default)
        {
            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicySets (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicySets PRIMARY KEY AUTOINCREMENT,
                    PolicyGroupKey TEXT NOT NULL,
                    PolicyCode TEXT NOT NULL,
                    PolicyName TEXT NOT NULL,
                    PolicyType TEXT NOT NULL DEFAULT 'Combined',
                    VersionNo INTEGER NOT NULL DEFAULT 1,
                    Status TEXT NOT NULL DEFAULT 'Draft',
                    IsLocked INTEGER NOT NULL DEFAULT 0,
                    EffectiveFrom TEXT NULL,
                    EffectiveTo TEXT NULL,
                    ApprovedDate TEXT NULL,
                    SourceTitle TEXT NULL,
                    SourceReference TEXT NULL,
                    Notes TEXT NULL,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastModifiedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicySectionDefinitions (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicySectionDefinitions PRIMARY KEY AUTOINCREMENT,
                    PolicySetId INTEGER NOT NULL,
                    SectionCode TEXT NOT NULL,
                    Heading TEXT NOT NULL,
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastModifiedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT FK_tblPolicySectionDefinitions_tblPolicySets_PolicySetId
                        FOREIGN KEY (PolicySetId) REFERENCES tblPolicySets (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyClauses (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyClauses PRIMARY KEY AUTOINCREMENT,
                    PolicySetId INTEGER NOT NULL,
                    ParentClauseId INTEGER NULL,
                    ClauseCode TEXT NULL,
                    Heading TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    PolicySection TEXT NOT NULL DEFAULT 'General',
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastModifiedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT FK_tblPolicyClauses_tblPolicySets_PolicySetId
                        FOREIGN KEY (PolicySetId) REFERENCES tblPolicySets (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_tblPolicyClauses_tblPolicyClauses_ParentClauseId
                        FOREIGN KEY (ParentClauseId) REFERENCES tblPolicyClauses (Id) ON DELETE RESTRICT
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyParameters (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyParameters PRIMARY KEY AUTOINCREMENT,
                    PolicySetId INTEGER NOT NULL,
                    PolicyClauseId INTEGER NULL,
                    ParameterKey TEXT NULL,
                    Label TEXT NOT NULL,
                    ValueType TEXT NOT NULL DEFAULT 'Text',
                    ValueText TEXT NULL,
                    DefaultValueText TEXT NULL,
                    Unit TEXT NULL,
                    Description TEXT NULL,
                    MinValueText TEXT NULL,
                    MaxValueText TEXT NULL,
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastModifiedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT FK_tblPolicyParameters_tblPolicySets_PolicySetId
                        FOREIGN KEY (PolicySetId) REFERENCES tblPolicySets (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_tblPolicyParameters_tblPolicyClauses_PolicyClauseId
                        FOREIGN KEY (PolicyClauseId) REFERENCES tblPolicyClauses (Id) ON DELETE SET NULL
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyLookupTables (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyLookupTables PRIMARY KEY AUTOINCREMENT,
                    PolicySetId INTEGER NOT NULL,
                    PolicyClauseId INTEGER NULL,
                    TableKey TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT NULL,
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastModifiedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT FK_tblPolicyLookupTables_tblPolicySets_PolicySetId
                        FOREIGN KEY (PolicySetId) REFERENCES tblPolicySets (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_tblPolicyLookupTables_tblPolicyClauses_PolicyClauseId
                        FOREIGN KEY (PolicyClauseId) REFERENCES tblPolicyClauses (Id) ON DELETE SET NULL
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyLookupColumns (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyLookupColumns PRIMARY KEY AUTOINCREMENT,
                    PolicyLookupTableId INTEGER NOT NULL,
                    ColumnKey TEXT NOT NULL,
                    HeaderText TEXT NOT NULL,
                    ValueType TEXT NOT NULL DEFAULT 'Text',
                    Unit TEXT NULL,
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    CONSTRAINT FK_tblPolicyLookupColumns_tblPolicyLookupTables_PolicyLookupTableId
                        FOREIGN KEY (PolicyLookupTableId) REFERENCES tblPolicyLookupTables (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyLookupRows (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyLookupRows PRIMARY KEY AUTOINCREMENT,
                    PolicyLookupTableId INTEGER NOT NULL,
                    DisplayOrder INTEGER NOT NULL DEFAULT 0,
                    RowLabel TEXT NULL,
                    CONSTRAINT FK_tblPolicyLookupRows_tblPolicyLookupTables_PolicyLookupTableId
                        FOREIGN KEY (PolicyLookupTableId) REFERENCES tblPolicyLookupTables (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyLookupCells (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyLookupCells PRIMARY KEY AUTOINCREMENT,
                    PolicyLookupRowId INTEGER NOT NULL,
                    PolicyLookupColumnId INTEGER NOT NULL,
                    ValueText TEXT NULL,
                    CONSTRAINT FK_tblPolicyLookupCells_tblPolicyLookupRows_PolicyLookupRowId
                        FOREIGN KEY (PolicyLookupRowId) REFERENCES tblPolicyLookupRows (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_tblPolicyLookupCells_tblPolicyLookupColumns_PolicyLookupColumnId
                        FOREIGN KEY (PolicyLookupColumnId) REFERENCES tblPolicyLookupColumns (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyAttachments (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyAttachments PRIMARY KEY AUTOINCREMENT,
                    PolicySetId INTEGER NOT NULL,
                    PolicyClauseId INTEGER NULL,
                    FileName TEXT NOT NULL,
                    ContentType TEXT NOT NULL DEFAULT 'image/png',
                    ImageData BLOB NOT NULL DEFAULT X'',
                    Caption TEXT NULL,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT FK_tblPolicyAttachments_tblPolicySets_PolicySetId
                        FOREIGN KEY (PolicySetId) REFERENCES tblPolicySets (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_tblPolicyAttachments_tblPolicyClauses_PolicyClauseId
                        FOREIGN KEY (PolicyClauseId) REFERENCES tblPolicyClauses (Id) ON DELETE SET NULL
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblPolicyAuditEntries (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblPolicyAuditEntries PRIMARY KEY AUTOINCREMENT,
                    PolicySetId INTEGER NOT NULL,
                    Action TEXT NOT NULL,
                    Details TEXT NULL,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    Actor TEXT NULL,
                    CONSTRAINT FK_tblPolicyAuditEntries_tblPolicySets_PolicySetId
                        FOREIGN KEY (PolicySetId) REFERENCES tblPolicySets (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_tblPolicySets_PolicyGroupKey_VersionNo ON tblPolicySets (PolicyGroupKey, VersionNo);",
                ct);
            await Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_tblPolicySectionDefinitions_PolicySetId_SectionCode ON tblPolicySectionDefinitions (PolicySetId, SectionCode);",
                ct);
        }
    }
}
