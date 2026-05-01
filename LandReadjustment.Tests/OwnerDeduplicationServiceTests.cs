using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using Xunit;

namespace LandReadjustment.Tests;

public class OwnerDeduplicationServiceTests
{
    [Fact]
    public void ExtractUniqueOwners_SameCitizenshipDifferentSpelling_MergesAutomatically()
    {
        var records = new List<BaselineLandParcelRecord>
        {
            new()
            {
                ParcelNo = "100",
                MapSheetNo = "01",
                LandOwnersName = "Ram Bahadur Thapa",
                FatherSpouse = "Hari Thapa",
                CitizenshipNumber = "12345",
                CitizenshipIssuedDate = "2020-01-01",
                AreaInSqm = 500.0
            },
            new()
            {
                ParcelNo = "200",
                MapSheetNo = "01",
                LandOwnersName = "Ram Bahadur  Thapa",
                FatherSpouse = "Hari Thapa",
                CitizenshipNumber = "12345",
                CitizenshipIssuedDate = "2020-01-01",
                AreaInSqm = 300.0
            }
        };

        var result = OwnerDeduplicationService.ExtractUniqueOwners(records);

        Assert.Equal(2, result.TotalOriginalRecords);
        Assert.Single(result.UniqueOwners);
        Assert.True(result.AutoMergedCount >= 1, "Should auto-merge on matching citizenship number");
    }

    [Fact]
    public void ExtractUniqueOwners_DifferentOwners_KeepsSeparate()
    {
        var records = new List<BaselineLandParcelRecord>
        {
            new()
            {
                ParcelNo = "100",
                MapSheetNo = "01",
                LandOwnersName = "Sita Devi Sharma",
                FatherSpouse = "Krishna Sharma",
                CitizenshipNumber = "11111",
                AreaInSqm = 400.0
            },
            new()
            {
                ParcelNo = "200",
                MapSheetNo = "01",
                LandOwnersName = "Gita Kumari Rai",
                FatherSpouse = "Bir Bahadur Rai",
                CitizenshipNumber = "22222",
                AreaInSqm = 600.0
            }
        };

        var result = OwnerDeduplicationService.ExtractUniqueOwners(records);

        Assert.Equal(2, result.TotalOriginalRecords);
        Assert.Equal(2, result.UniqueOwners.Count);
        Assert.Equal(0, result.AutoMergedCount);
    }
}
