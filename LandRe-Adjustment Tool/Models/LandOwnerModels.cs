namespace Land_Readjustment_Tool.Models
{
    /// <summary>
    /// Represents a unique landowner in the system
    /// Contains only owner-specific information (personal details, identification, contact)
    /// </summary>
    public class LandOwner
    {
        public int LandOwnerId { get; set; }
        public string LandOwnersName { get; set; } = string.Empty;
        public string? FatherSpouse { get; set; }
        public string? Gender { get; set; }
        public string? CitizenshipNumber { get; set; }
        public string? CitizenshipIssuedDistrict { get; set; }
        public string? CitizenshipIssuedDate { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? ContactNumber { get; set; }
        public string? EmailID { get; set; }
        public string? PhotoPath { get; set; }
        public string? DocumentsFolderPath { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Gets unique identifier for this owner (for deduplication)
        /// </summary>
        public string GetUniqueKey()
        {
            return $"{LandOwnersName?.Trim().ToUpper()}|{FatherSpouse?.Trim().ToUpper()}|{CitizenshipNumber?.Trim()}";
        }
    }

    /// <summary>
    /// Represents a land parcel with reference to its owner
    /// Contains all parcel-specific information including location details
    /// </summary>
    public class OriginalLandParcel
    {
        public int ParcelId { get; set; }
        public int LandOwnerId { get; set; }
        public string ParcelNo { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? MunicipalityVillage { get; set; }
        public string? WardNo { get; set; }
        public string? ParcelLocation { get; set; }
        public string MapSheetNo { get; set; } = string.Empty;
        public string? IsTenant { get; set; }
        public string? LandUse { get; set; }
        public string? LandOwnershipType { get; set; }
        public double? AreaInSqm { get; set; }
        public string? AreaInRAPD { get; set; }
        public string? AreaInBKD { get; set; }
        public string? MothNo { get; set; }
        public string? PaanaNo { get; set; }
        public string? Remarks { get; set; }
        public DateTime ImportedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsValid { get; set; } = true;
        public string? ValidationErrors { get; set; }

        // Navigation property
        public LandOwner? Owner { get; set; }
    }

    /// <summary>
    /// Display model for showing parcel and owner information in DataGridView
    /// Flattens the Owner navigation property for grid binding
    /// </summary>
    public class ParcelOwnerDisplayModel
    {
        public int ParcelId { get; set; }
        public int LandOwnerId { get; set; }
        public string ParcelNo { get; set; } = string.Empty;
        public string MapSheetNo { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string MunicipalityVillage { get; set; } = string.Empty;
        public string LandOwnersName { get; set; } = string.Empty;
        public string FatherSpouse { get; set; } = string.Empty;
        public string CitizenshipNumber { get; set; } = string.Empty;
        public string CitizenshipIssueDate { get; set; } = string.Empty;
        public string citizenshipIssueDistrict { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string ParcelLocation { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public double? AreaInSqm { get; set; }
        public string AreaInRAPD { get; set; } = string.Empty;
        public string AreaInBKD { get; set; } = string.Empty;
        public string LandUse { get; set; } = string.Empty;
        public string IsTenant { get; set; } = string.Empty;
        public string MothNo { get; set; } = string.Empty;
        public string PaanaNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
