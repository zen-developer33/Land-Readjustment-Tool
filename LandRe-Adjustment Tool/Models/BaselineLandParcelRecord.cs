using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Models
{
    // Raw / Dirty data model imported from external sources
    [NotMapped]
    public class BaselineLandParcelRecord
    {
        /* ===============================
           Parcel Identification
           =============================== */

        [Category("Parcel Identification")]
        [Description("Original parcel or kitta number as recorded in source data")]
        public string? ParcelNo { get; set; }

        [Category("Parcel Identification")]
        [Description("Map sheet or cadastral sheet number")]
        public string? MapSheetNo { get; set; }

        [Category("Parcel Identification")]
        [Description("Local name or description of parcel location")]
        public string? ParcelLocation { get; set; }

        /* ===============================
           Administrative Information
           =============================== */

        [Category("Administrative")]
        [Description("Province where the parcel is located")]
        public string? Province { get; set; }

        [Category("Administrative")]
        [Description("District name")]
        public string? District { get; set; }

        [Category("Administrative")]
        [Description("Municipality or Village (Gaupalika / Nagarpalika)")]
        public string? MunicipalityVillage { get; set; }

        [Category("Administrative")]
        [Description("Ward number")]
        public string? WardNo { get; set; }

        /* ===============================
           Ownership Information (Raw)
           =============================== */

        [Category("Owner Details")]
        [Description("Land owner's name as written in original record")]
        public string? LandOwnersName { get; set; }

        [Category("Owner Details")]
        [Description("Father or spouse name of land owner")]
        public string? FatherSpouse { get; set; }

        [Category("Owner Details")]
        [Description("Gender of land owner if available")]
        public string? Gender { get; set; }

        /* ===============================
           Citizenship Information
           =============================== */

        [Category("Citizenship")]
        [Description("Citizenship certificate number as recorded")]
        public string? CitizenshipNumber { get; set; }

        [Category("Citizenship")]
        [Description("District from which citizenship was issued")]
        public string? CitizenshipIssuedDistrict { get; set; }

        [Category("Citizenship")]
        [Description("Citizenship issued date (raw value from source)")]
        public string? CitizenshipIssuedDate { get; set; }

        /* ===============================
           Address & Contact
           =============================== */

        [Category("Address")]
        [Description("Permanent address of land owner")]
        public string? PermanentAddress { get; set; }

        [Category("Address")]
        [Description("Temporary address of land owner")]
        public string? TemporaryAddress { get; set; }

        [Category("Conatct Information")]
        [Description("Contact number of land owner")]
        public string? ContactNumber { get; set; }

        [Description("E-mail Address of land owner")]
        public string? EmailID { get; set; }

        /* ===============================
           Other Parcel Information
           =============================== */

        [Category("Tenancy")]
        [Description("Indicates whether the parcel has a tenant (Mohi)")]
        public string? Tenant { get; set; }

        [Category("Land Classification")]
        [Description("Land use type (Residential, Agricultural, etc.)")]
        public string? LandUse { get; set; }

        [Category("Land Classification")]
        [Description("Ownership type (Individual, Joint, Guthi, etc.)")]
        public string? LandOwnershipType { get; set; } // Single, Joint, Government, Guthi,etc.

        /* ===============================
           Area Information (As Recorded)
           =============================== */

        [Category("Area")]
        [Description("Area of land in square meters")]
        public double? AreaInSqm { get; set; }

        [Category("Area")]
        [Description("Area expressed in Local Land Measurement Unit(Ropani-Aana-Paisa-Dam) format")]
        public string? AreaInRAPD { get; set; }

        [Category("Area")]
        [Description("Area expressed in Local Land Measurement Unit(Bigha-Kattha-Dhur) format")]
        public string? AreaInBKD { get; set; }

        /* ===============================
           Registry References
           =============================== */

        [Category("Registry References")]
        [Description("Book number from Official land registry")]
        public string? MothNo { get; set; }

        [Category("Registry References")]
        [Description("Page number from Official land registry Book")]
        public string? PaanaNo { get; set; }

        /* ===============================
           Remarks
           =============================== */

        [Category("Remarks")]
        [Description("Additional remarks or notes from source data")]
        public string? Remarks { get; set; }
    }
}
