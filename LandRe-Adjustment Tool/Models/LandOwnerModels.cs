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
        public string? ContactNumber { get; set; }
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
}
