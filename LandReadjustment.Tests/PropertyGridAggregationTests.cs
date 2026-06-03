using System.Collections;
using System.Reflection;
using Land_Readjustment_Tool;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Layout;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class PropertyGridAggregationTests
{
    [Fact]
    public void MultipleSelection_ShowsSharedFieldValueWhenObjectsMatch()
    {
        object layerField = GetParcelPropertyField("selection.layer");
        CanvasLayer layer = new() { Name = "Residential Block", LayerType = "Block" };
        CanvasObject first = new() { ObjectType = "Polygon", CanvasLayer = layer };
        CanvasObject second = new() { ObjectType = "Polygon", CanvasLayer = layer };

        string? value = InvokeGetAggregatedFieldValue([first, second], layerField, selectedCount: 2);

        Assert.Equal("Residential Block", value);
    }

    [Fact]
    public void MultipleSelection_ShowsVariesOnlyWhenFieldValuesDiffer()
    {
        object layerField = GetParcelPropertyField("selection.layer");
        CanvasObject first = new()
        {
            ObjectType = "Polygon",
            CanvasLayer = new CanvasLayer { Name = "Residential Block", LayerType = "Block" }
        };
        CanvasObject second = new()
        {
            ObjectType = "Polygon",
            CanvasLayer = new CanvasLayer { Name = "Open Space", LayerType = "Block" }
        };

        string? value = InvokeGetAggregatedFieldValue([first, second], layerField, selectedCount: 2);

        Assert.Equal("*VARIES*", value);
    }

    [Fact]
    public void MultipleSelection_SumsBlockAreaInsteadOfShowingVaries()
    {
        object blockAreaField = GetParcelPropertyField("block.area");
        CanvasObject first = new()
        {
            ObjectType = "Polygon",
            Block = new Block { BlockArea = 100 }
        };
        CanvasObject second = new()
        {
            ObjectType = "Polygon",
            Block = new Block { BlockArea = 250 }
        };

        string? value = InvokeGetAggregatedFieldValue([first, second], blockAreaField, selectedCount: 2);

        Assert.NotNull(value);
        Assert.StartsWith("350.000 sq.m", value);
    }

    private static object GetParcelPropertyField(string key)
    {
        MethodInfo method = typeof(frmMain).GetMethod(
            "GetParcelPropertyFields",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        IEnumerable fields = (IEnumerable)method.Invoke(null, null)!;

        foreach (object field in fields)
        {
            string? fieldKey = field.GetType().GetProperty("Key")?.GetValue(field) as string;
            if (string.Equals(fieldKey, key, StringComparison.OrdinalIgnoreCase))
                return field;
        }

        throw new InvalidOperationException($"Property field '{key}' was not found.");
    }

    private static string? InvokeGetAggregatedFieldValue(
        IReadOnlyList<CanvasObject> selectedObjects,
        object field,
        int selectedCount)
    {
        MethodInfo method = typeof(frmMain).GetMethod(
            "GetAggregatedFieldValue",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        return (string?)method.Invoke(null, [selectedObjects, field, selectedCount]);
    }
}
