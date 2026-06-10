using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Pooling_Policy_Manager.Entities.Policy
{
    // ──────────────────────────────────────────────────────────────────────────
    // Phase 1 slice: entities only, mirroring the schema that currently lives in
    // the main project's AppDbContext (tables prefixed tblPolicy*). The new
    // PolicyDbContext (Data/PolicyDbContext.cs) maps these to the same physical
    // tables, so when this manager opens a project's .lpp file it reads/writes
    // the same data. When standalone it writes to %AppData%\RePlot\PolicyManager
    // \policies.db, which the Program.cs creates on first run.
    //
    // Phase 2 will move services, seeder, forms, and migrations into this
    // project and remove the duplicates from the main project.
    // ──────────────────────────────────────────────────────────────────────────

    [Table("tblPolicySets")]
    public class PolicySet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string PolicyGroupKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(240)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string PolicyType { get; set; } = "Combined";

        public int VersionNo { get; set; } = 1;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = PolicyStatuses.Draft;

        public bool IsLocked { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [MaxLength(240)]
        public string? SourceTitle { get; set; }

        [MaxLength(500)]
        public string? SourceReference { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public ICollection<PolicyClause> Clauses { get; set; } = new List<PolicyClause>();
        public ICollection<PolicyParameter> Parameters { get; set; } = new List<PolicyParameter>();
        public ICollection<PolicyLookupTable> LookupTables { get; set; } = new List<PolicyLookupTable>();
        public ICollection<PolicyAttachment> Attachments { get; set; } = new List<PolicyAttachment>();
        public ICollection<PolicyAuditEntry> AuditEntries { get; set; } = new List<PolicyAuditEntry>();
        public ICollection<PolicySectionDefinition> Sections { get; set; } = new List<PolicySectionDefinition>();
    }

    [Table("tblPolicySectionDefinitions")]
    public class PolicySectionDefinition
    {
        [Key]
        public int Id { get; set; }

        public int PolicySetId { get; set; }

        [Required]
        [MaxLength(8)]
        public string SectionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string Heading { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public PolicySet PolicySet { get; set; } = null!;
    }

    [Table("tblPolicyClauses")]
    public class PolicyClause
    {
        [Key]
        public int Id { get; set; }

        public int PolicySetId { get; set; }
        public int? ParentClauseId { get; set; }

        [MaxLength(80)]
        public string? ClauseCode { get; set; }

        [Required]
        [MaxLength(300)]
        public string Heading { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string PolicySection { get; set; } = "General";

        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public PolicySet PolicySet { get; set; } = null!;
        public PolicyClause? ParentClause { get; set; }
        public ICollection<PolicyClause> ChildClauses { get; set; } = new List<PolicyClause>();
        public ICollection<PolicyParameter> Parameters { get; set; } = new List<PolicyParameter>();
        public ICollection<PolicyAttachment> Attachments { get; set; } = new List<PolicyAttachment>();
    }

    [Table("tblPolicyParameters")]
    public class PolicyParameter
    {
        [Key]
        public int Id { get; set; }

        public int PolicySetId { get; set; }
        public int? PolicyClauseId { get; set; }

        [MaxLength(120)]
        public string? ParameterKey { get; set; }

        [Required]
        [MaxLength(180)]
        public string Label { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string ValueType { get; set; } = "Text";

        public string? ValueText { get; set; }
        public string? DefaultValueText { get; set; }

        [MaxLength(40)]
        public string? Unit { get; set; }

        public string? Description { get; set; }
        public string? MinValueText { get; set; }
        public string? MaxValueText { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public PolicySet PolicySet { get; set; } = null!;
        public PolicyClause? PolicyClause { get; set; }
    }

    [Table("tblPolicyLookupTables")]
    public class PolicyLookupTable
    {
        [Key]
        public int Id { get; set; }

        public int PolicySetId { get; set; }
        public int? PolicyClauseId { get; set; }

        [Required]
        [MaxLength(120)]
        public string TableKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(220)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public PolicySet PolicySet { get; set; } = null!;
        public PolicyClause? PolicyClause { get; set; }
        public ICollection<PolicyLookupColumn> Columns { get; set; } = new List<PolicyLookupColumn>();
        public ICollection<PolicyLookupRow> Rows { get; set; } = new List<PolicyLookupRow>();
    }

    [Table("tblPolicyLookupColumns")]
    public class PolicyLookupColumn
    {
        [Key]
        public int Id { get; set; }

        public int PolicyLookupTableId { get; set; }

        [Required]
        [MaxLength(120)]
        public string ColumnKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string HeaderText { get; set; } = string.Empty;

        [MaxLength(40)]
        public string ValueType { get; set; } = "Text";

        [MaxLength(40)]
        public string? Unit { get; set; }

        public int DisplayOrder { get; set; }

        public PolicyLookupTable PolicyLookupTable { get; set; } = null!;
        public ICollection<PolicyLookupCell> Cells { get; set; } = new List<PolicyLookupCell>();
    }

    [Table("tblPolicyLookupRows")]
    public class PolicyLookupRow
    {
        [Key]
        public int Id { get; set; }

        public int PolicyLookupTableId { get; set; }
        public int DisplayOrder { get; set; }

        [MaxLength(240)]
        public string? RowLabel { get; set; }

        public PolicyLookupTable PolicyLookupTable { get; set; } = null!;
        public ICollection<PolicyLookupCell> Cells { get; set; } = new List<PolicyLookupCell>();
    }

    [Table("tblPolicyLookupCells")]
    public class PolicyLookupCell
    {
        [Key]
        public int Id { get; set; }

        public int PolicyLookupRowId { get; set; }
        public int PolicyLookupColumnId { get; set; }
        public string? ValueText { get; set; }

        public PolicyLookupRow PolicyLookupRow { get; set; } = null!;
        public PolicyLookupColumn PolicyLookupColumn { get; set; } = null!;
    }

    [Table("tblPolicyAttachments")]
    public class PolicyAttachment
    {
        [Key]
        public int Id { get; set; }

        public int PolicySetId { get; set; }
        public int? PolicyClauseId { get; set; }

        [Required]
        [MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string ContentType { get; set; } = "image/png";

        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        [MaxLength(500)]
        public string? Caption { get; set; }

        public DateTime CreatedDate { get; set; }

        public PolicySet PolicySet { get; set; } = null!;
        public PolicyClause? PolicyClause { get; set; }
    }

    [Table("tblPolicyAuditEntries")]
    public class PolicyAuditEntry
    {
        [Key]
        public int Id { get; set; }

        public int PolicySetId { get; set; }

        [Required]
        [MaxLength(60)]
        public string Action { get; set; } = string.Empty;

        public string? Details { get; set; }
        public DateTime CreatedDate { get; set; }

        [MaxLength(120)]
        public string? Actor { get; set; }

        public PolicySet PolicySet { get; set; } = null!;
    }

    public static class PolicyStatuses
    {
        public const string Draft = "Draft";
        public const string Approved = "Approved";
        public const string Archived = "Archived";
    }
}
