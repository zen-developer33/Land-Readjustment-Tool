using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Models
{
    /// <summary>
    /// Transient model representing a co-owner on a jointly-owned baseline parcel.
    /// Used during import to carry additional owner data before persistence.
    /// Stored in BaselineLandParcelRecord.JointCoOwners — never mapped directly to a table.
    /// </summary>
    [NotMapped]
    public class CoOwnerRecord
    {
        public string? OwnerName { get; set; }
        public string? FatherSpouse { get; set; }
        public string? Gender { get; set; }
        public string? CitizenshipNumber { get; set; }
        public string? CitizenshipIssuedDistrict { get; set; }
        public string? CitizenshipIssuedDate { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? ContactNumber { get; set; }
        public string? EmailID { get; set; }
        // null = equal/unknown share
        public double? OwnershipSharePercent { get; set; }

        public override string ToString() =>
            string.IsNullOrWhiteSpace(OwnerName) ? "(Unknown)" : OwnerName;
    }
}
