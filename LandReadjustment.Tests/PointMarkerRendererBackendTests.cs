using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using Xunit;

namespace LandReadjustment.Tests;

/// <summary>
/// Tests point marker drawing through the backend-neutral surface contract.
/// </summary>
public sealed class PointMarkerRendererBackendTests
{
    /// <summary>
    /// Verifies that path-based marker symbols render visible pixels through
    /// the backend surface.
    /// </summary>
    [Theory]
    [InlineData("Diamond")]
    [InlineData("Triangle")]
    [InlineData("Star")]
    public void Draw_PathMarker_RendersVisiblePixels(string markerKey)
    {
        using Bitmap bitmap = new(80, 80);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using GdiMapRenderSurface surface = new(graphics, bitmap.Size);
        surface.Clear(Color.White);

        PointMarkerRenderer.Draw(
            surface,
            new RectangleF(15, 15, 50, 50),
            markerKey,
            Color.Blue,
            3.0f);

        Assert.True(CountNonWhitePixels(bitmap) > 100);
    }

    /// <summary>
    /// Counts pixels that differ from the white test background.
    /// </summary>
    private static int CountNonWhitePixels(Bitmap bitmap)
    {
        int white = Color.White.ToArgb();
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != white)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
