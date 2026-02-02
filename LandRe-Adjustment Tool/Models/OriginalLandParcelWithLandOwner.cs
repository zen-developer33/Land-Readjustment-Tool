namespace Land_Readjustment_Tool.Models
{
    //This is the model class for dirty data of Original Land Owners Record
    public class OriginalLandParcelWithLandOwner
    {
        public string? ParcelNo { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? MunicipalityVillage { get; set; }
        public string? WardNo { get; set; }
        public string? ParcelLocation { get; set; }
        public string? MapSheetNo { get; set; }
        public string? LandOwnersName { get; set; }
        public string? FatherSpouse { get; set; }
        public string? Gender { get; set; }
        public string? CitizenshipNumber { get; set; }
        public string? CitizenshipIssuedDistrict { get; set; }
        public string? citizenshipIssuedDate { get; set; }
        public string? IsTenant { get; set; }
        public string? PermanentAddress { get; set; }
        public string? LandUse { get; set; }
        public double? AreaInSqm { get; set; }
        public string? AreaInRAPD { get; set; }
        public string? AreaInBKD { get; set; }
        public string? MothNo { get; set; }
        public string? PaanaNo { get; set; }
        public string? Remarks { get; set; }

    }

}
