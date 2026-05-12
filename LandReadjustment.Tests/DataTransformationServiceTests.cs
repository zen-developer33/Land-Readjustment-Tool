using System.Data;
using Land_Readjustment_Tool.Services;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class DataTransformationServiceTests
{
    [Fact]
    public void ValidateFromDataTable_IgnoresTransientJointOwnershipColumns()
    {
        DataTable table = new();
        table.Columns.Add("ParcelNo", typeof(string));
        table.Columns.Add("MapSheetNo", typeof(string));
        table.Columns.Add("AreaInSqm", typeof(double));
        table.Columns.Add("JointCoOwners", typeof(string));
        table.Columns.Add("IsJointCoOwnerRow", typeof(string));

        DataRow row = table.NewRow();
        row["ParcelNo"] = "101";
        row["MapSheetNo"] = "Sheet-1";
        row["AreaInSqm"] = 100.0;
        row["JointCoOwners"] = "Raw UI text should not be mapped";
        row["IsJointCoOwnerRow"] = "False";
        table.Rows.Add(row);

        TransformationResult result = DataTransformationService.ValidateFromDataTable(table);

        Assert.False(result.HasErrors);
        Assert.Single(result.ValidRecords);
        Assert.Empty(result.ValidRecords[0].JointCoOwners);
        Assert.False(result.ValidRecords[0].IsJointCoOwnerRow);
    }

    [Fact]
    public void ValidateFromDataTable_MarksOnlyDuplicateParcelRowsForJointOwnership()
    {
        DataTable table = new();
        table.Columns.Add("ParcelNo", typeof(string));
        table.Columns.Add("MapSheetNo", typeof(string));
        table.Columns.Add("AreaInSqm", typeof(double));

        table.Rows.Add("101", "Sheet-1", 100.0);
        table.Rows.Add("101", "Sheet-1", 150.0);

        TransformationResult result = DataTransformationService.ValidateFromDataTable(table);

        Assert.Equal(2, result.ValidationErrors.Count);
        Assert.All(result.ValidationErrors, error => Assert.True(error.IsDuplicateParcel));
        Assert.Single(result.ValidationErrors.Select(error => error.DuplicateParcelKey).Distinct());
    }
}
