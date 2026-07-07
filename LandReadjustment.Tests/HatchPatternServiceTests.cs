using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class HatchPatternServiceTests
{
    [Fact]
    public void GetPatterns_ReturnsExpectedCatalog()
    {
        HatchPatternService service = new();

        IReadOnlyList<HatchPatternDefinition> patterns = service.GetPatterns();

        Assert.True(patterns.Count >= 18);
        Assert.Contains(patterns, pattern => pattern.Key == "ANSI31");
        Assert.Contains(patterns, pattern => pattern.Key == "DIAGONAL-CROSS");
        Assert.Contains(patterns, pattern => pattern.Key == "GRASS");
        Assert.Contains(patterns, pattern => pattern.Key == "WATER");
        Assert.Equal("ANSI31", service.GetPatternOrDefault("missing").Key);
    }

    [Fact]
    public void DrawPreview_RendersPatternPixels()
    {
        HatchPatternService service = new();
        using Bitmap bitmap = new(64, 64);
        using Graphics graphics = Graphics.FromImage(bitmap);

        service.DrawPreview(
            graphics,
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            "DOTS",
            Color.Black,
            Color.White,
            0,
            1.0,
            Color.White);

        bool hasDarkPixel = false;
        for (int y = 0; y < bitmap.Height && !hasDarkPixel; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.R < 120 && pixel.G < 120 && pixel.B < 120)
                {
                    hasDarkPixel = true;
                    break;
                }
            }
        }

        Assert.True(hasDarkPixel);
    }

    [Fact]
    public void DrawPreview_AllCatalogPatterns_RenderVisiblePatternPixels()
    {
        HatchPatternService service = new();

        foreach (HatchPatternDefinition pattern in service.GetPatterns())
        {
            using Bitmap bitmap = new(72, 72);
            using Graphics graphics = Graphics.FromImage(bitmap);
            service.DrawPreview(
                graphics,
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                pattern.Key,
                Color.Black,
                Color.White,
                0,
                1.0,
                Color.White);

            Assert.True(HasDarkPixel(bitmap), $"Pattern {pattern.Key} did not render visible hatch pixels.");
        }
    }

    private static bool HasDarkPixel(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.R < 120 && pixel.G < 120 && pixel.B < 120)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
