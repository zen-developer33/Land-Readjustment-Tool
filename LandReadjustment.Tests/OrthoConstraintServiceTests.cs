using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class OrthoConstraintServiceTests
{
    [Fact]
    public void ConstrainToDominantAxis_WhenHorizontalMovementDominates_KeepsAnchorY()
    {
        PointD anchor = new(10.0, 20.0);
        PointD candidate = new(16.0, 23.0);

        PointD constrained = OrthoConstraintService.ConstrainToDominantAxis(anchor, candidate);

        Assert.Equal(16.0, constrained.X);
        Assert.Equal(20.0, constrained.Y);
    }

    [Fact]
    public void ConstrainToDominantAxis_WhenVerticalMovementDominates_KeepsAnchorX()
    {
        PointD anchor = new(10.0, 20.0);
        PointD candidate = new(12.0, 29.0);

        PointD constrained = OrthoConstraintService.ConstrainToDominantAxis(anchor, candidate);

        Assert.Equal(10.0, constrained.X);
        Assert.Equal(29.0, constrained.Y);
    }

    [Fact]
    public void ConstrainToDominantAxis_WhenMouseIsLeftOfAnchor_ProjectsHorizontally()
    {
        PointD anchor = new(10.0, 20.0);
        PointD candidate = new(3.0, 18.0);

        PointD constrained = OrthoConstraintService.ConstrainToDominantAxis(anchor, candidate);

        Assert.Equal(3.0, constrained.X);
        Assert.Equal(20.0, constrained.Y);
    }

    [Fact]
    public void ConstrainToDominantAxis_WhenMouseIsAboveAnchor_ProjectsVertically()
    {
        PointD anchor = new(10.0, 20.0);
        PointD candidate = new(12.0, 26.0);

        PointD constrained = OrthoConstraintService.ConstrainToDominantAxis(anchor, candidate);

        Assert.Equal(10.0, constrained.X);
        Assert.Equal(26.0, constrained.Y);
    }
}
