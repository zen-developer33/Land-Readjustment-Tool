using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services.Import;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class ParcelDuplicateResolutionServiceTests
{
    [Fact]
    public void ApplyJointOwnership_MovesDuplicateRowsIntoPrimaryCoOwners()
    {
        var records = new List<BaselineLandParcelRecord>
        {
            new()
            {
                ParcelNo = "101",
                MapSheetNo = "S1",
                LandOwnersName = "Ram Thapa",
                FatherSpouse = "Hari Thapa",
                CitizenshipNumber = "111",
                AreaInSqm = 100
            },
            new()
            {
                ParcelNo = "101",
                MapSheetNo = "S1",
                LandOwnersName = "Sita Thapa",
                FatherSpouse = "Gopal Thapa",
                CitizenshipNumber = "222",
                AreaInSqm = 100
            }
        };

        var group = Assert.Single(ParcelDuplicateResolutionService.FindDuplicateGroups(records));
        ParcelDuplicateResolutionService.ApplyJointOwnership(group, primaryIndex: 0);

        Assert.Equal("Private (Joint)", records[0].LandOwnershipType);
        Assert.False(records[0].IsJointCoOwnerRow);
        Assert.True(records[1].IsJointCoOwnerRow);
        var coOwner = Assert.Single(records[0].JointCoOwners);
        Assert.Equal("Sita Thapa", coOwner.OwnerName);
        Assert.Equal("222", coOwner.CitizenshipNumber);
    }

    [Fact]
    public void ApplyJointOwnership_RecordListSupportsManualSelectedRows()
    {
        var records = new List<BaselineLandParcelRecord>
        {
            new()
            {
                ParcelNo = "101",
                MapSheetNo = "S1",
                LandOwnersName = "Ram Thapa",
                FatherSpouse = "Hari Thapa",
                CitizenshipNumber = "111"
            },
            new()
            {
                ParcelNo = "102",
                MapSheetNo = "S1",
                LandOwnersName = "Sita Thapa",
                FatherSpouse = "Gopal Thapa",
                CitizenshipNumber = "222"
            },
            new()
            {
                ParcelNo = "103",
                MapSheetNo = "S1",
                LandOwnersName = "Gita Thapa",
                FatherSpouse = "Mohan Thapa",
                CitizenshipNumber = "333"
            }
        };

        ParcelDuplicateResolutionService.ApplyJointOwnership(records, primaryIndex: 1);

        Assert.True(records[0].IsJointCoOwnerRow);
        Assert.False(records[1].IsJointCoOwnerRow);
        Assert.True(records[2].IsJointCoOwnerRow);
        Assert.Equal("Private (Joint)", records[1].LandOwnershipType);
        Assert.Equal(new[] { "Ram Thapa", "Gita Thapa" }, records[1].JointCoOwners.Select(o => o.OwnerName).ToArray());
    }

    [Fact]
    public void ApplyJointOwnership_SkipsCoOwnerWhenItMatchesPrimaryOwner()
    {
        var records = new List<BaselineLandParcelRecord>
        {
            new()
            {
                ParcelNo = "101",
                MapSheetNo = "S1",
                LandOwnersName = "Ram Thapa",
                FatherSpouse = "Hari Thapa",
                CitizenshipNumber = "111"
            },
            new()
            {
                ParcelNo = "101",
                MapSheetNo = "S1",
                LandOwnersName = "Ram Thapa",
                FatherSpouse = "Hari Thapa",
                CitizenshipNumber = "111"
            }
        };

        ParcelDuplicateResolutionService.ApplyJointOwnership(records, primaryIndex: 0);

        Assert.Empty(records[0].JointCoOwners);
        Assert.True(records[1].IsJointCoOwnerRow);
    }
}
