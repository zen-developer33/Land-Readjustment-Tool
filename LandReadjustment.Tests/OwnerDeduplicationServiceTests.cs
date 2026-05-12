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

    [Fact]
    public void ExtractUniqueOwners_IncludesJointCoOwnersAndKeepsSourceReferences()
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
                AreaInSqm = 500.0,
                JointCoOwners =
                {
                    new CoOwnerRecord
                    {
                        OwnerName = "Sita Devi Thapa",
                        FatherSpouse = "Gopal Thapa",
                        CitizenshipNumber = "999"
                    }
                }
            },
            new()
            {
                ParcelNo = "200",
                MapSheetNo = "01",
                LandOwnersName = "Sita Devi  Thapa",
                FatherSpouse = "Gopal Thapa",
                CitizenshipNumber = "999",
                AreaInSqm = 300.0
            }
        };

        var result = OwnerDeduplicationService.ExtractUniqueOwners(records);

        Assert.Equal(2, result.UniqueOwners.Count);
        var sita = Assert.Single(result.UniqueOwners, o => o.CitizenshipNumber == "999");
        Assert.Contains(sita.SourceOwners, source =>
            source.Kind == OwnerDeduplicationService.OwnerReferenceKind.CoOwner &&
            source.ParcelIndex == 0 &&
            source.CoOwnerIndex == 0);
        Assert.Contains(sita.SourceOwners, source =>
            source.Kind == OwnerDeduplicationService.OwnerReferenceKind.Primary &&
            source.ParcelIndex == 1);
    }

    [Fact]
    public void ApplyDeduplicationToRecords_UpdatesCoOwnerWithoutOverwritingPrimary()
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
                AreaInSqm = 500.0,
                JointCoOwners =
                {
                    new CoOwnerRecord
                    {
                        OwnerName = "Sita Devi  Thapa",
                        FatherSpouse = "Gopal Thapa",
                        CitizenshipNumber = "999"
                    }
                }
            }
        };

        var result = new OwnerDeduplicationService.DeduplicationResult
        {
            UniqueOwners =
            {
                new OwnerDeduplicationService.UniqueOwner
                {
                    LandOwnersName = "Sita Devi Thapa",
                    FatherSpouse = "Gopal Thapa",
                    CitizenshipNumber = "999",
                    ParcelIndices = new List<int> { 0 },
                    SourceOwners =
                    {
                        new OwnerDeduplicationService.OwnerReference
                        {
                            ParcelIndex = 0,
                            Kind = OwnerDeduplicationService.OwnerReferenceKind.CoOwner,
                            CoOwnerIndex = 0
                        }
                    }
                }
            }
        };

        OwnerDeduplicationService.ApplyDeduplicationToRecords(records, result);

        Assert.Equal("Ram Bahadur Thapa", records[0].LandOwnersName);
        Assert.Equal("Sita Devi Thapa", records[0].JointCoOwners[0].OwnerName);
    }

    [Fact]
    public void ExtractUniqueOwners_SourceReferencesKeepCorrectParcelsAfterListReorder()
    {
        var first = new BaselineLandParcelRecord
        {
            ParcelNo = "480",
            MapSheetNo = "1K",
            LandOwnersName = "श्री ५ सरकार",
            AreaInSqm = 100.0
        };
        var second = new BaselineLandParcelRecord
        {
            ParcelNo = "999",
            MapSheetNo = "9K",
            LandOwnersName = "श्री ५ सरकार",
            AreaInSqm = 200.0
        };
        var records = new List<BaselineLandParcelRecord> { first, second };

        var result = OwnerDeduplicationService.ExtractUniqueOwners(records);
        records.Reverse();

        var owner = Assert.Single(result.UniqueOwners);
        var parcelsFromSourceRecords = owner.SourceOwners
            .Select(source => source.Record?.ParcelNo)
            .OrderBy(parcelNo => parcelNo)
            .ToList();

        Assert.Equal(["480", "999"], parcelsFromSourceRecords);
    }
}
