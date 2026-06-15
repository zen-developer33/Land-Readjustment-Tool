namespace Land_Pooling_Policy_Manager.Services
{
    public sealed class PolicyPackage
    {
        public string SchemaVersion { get; set; } = "1.1";
        public DateTime ExportedAt { get; set; } = DateTime.Now;
        public PolicySetPackage Policy { get; set; } = new();
        public List<PolicySectionPackage> Sections { get; set; } = [];
        public List<PolicyClausePackage> Clauses { get; set; } = [];
        public List<PolicyParameterPackage> Parameters { get; set; } = [];
        public List<PolicyLookupTablePackage> LookupTables { get; set; } = [];
        public List<PolicyAttachmentPackage> Attachments { get; set; } = [];
    }

    public sealed class PolicySetPackage
    {
        public string PolicyGroupKey { get; set; } = string.Empty;
        public string PolicyCode { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyType { get; set; } = "Combined";
        public int VersionNo { get; set; } = 1;
        public string Status { get; set; } = "Draft";
        public bool IsLocked { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? SourceTitle { get; set; }
        public string? SourceReference { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class PolicySectionPackage
    {
        public string SectionCode { get; set; } = string.Empty;
        public string Heading { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public sealed class PolicyClausePackage
    {
        public int LocalId { get; set; }
        public int? ParentLocalId { get; set; }
        public string? ClauseCode { get; set; }
        public string Heading { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PolicySection { get; set; } = "General";
        public int DisplayOrder { get; set; }
    }

    public sealed class PolicyParameterPackage
    {
        public int? ClauseLocalId { get; set; }
        public string? ParameterKey { get; set; }
        public string Label { get; set; } = string.Empty;
        public string ValueType { get; set; } = "Text";
        public string? ValueText { get; set; }
        public string? DefaultValueText { get; set; }
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public string? MinValueText { get; set; }
        public string? MaxValueText { get; set; }
        public int DisplayOrder { get; set; }
    }

    public sealed class PolicyLookupTablePackage
    {
        public int? ClauseLocalId { get; set; }
        public string TableKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public List<PolicyLookupColumnPackage> Columns { get; set; } = [];
        public List<PolicyLookupRowPackage> Rows { get; set; } = [];
    }

    public sealed class PolicyLookupColumnPackage
    {
        public string ColumnKey { get; set; } = string.Empty;
        public string HeaderText { get; set; } = string.Empty;
        public string ValueType { get; set; } = "Text";
        public string? Unit { get; set; }
        public int DisplayOrder { get; set; }
    }

    public sealed class PolicyLookupRowPackage
    {
        public string? RowLabel { get; set; }
        public int DisplayOrder { get; set; }
        public Dictionary<string, string?> Values { get; set; } = [];
    }

    public sealed class PolicyAttachmentPackage
    {
        public int? ClauseLocalId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/png";
        public string ImageDataBase64 { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}
