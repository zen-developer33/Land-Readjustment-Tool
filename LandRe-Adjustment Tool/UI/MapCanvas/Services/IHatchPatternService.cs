using System.Drawing;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    public interface IHatchPatternService
    {
        IReadOnlyList<HatchPatternDefinition> GetPatterns();

        HatchPatternDefinition GetPatternOrDefault(string? key);

        void DrawPreview(
            Graphics graphics,
            Rectangle bounds,
            string? patternKey,
            Color hatchColor,
            Color fillColor,
            int transparency,
            double hatchScale,
            Color backgroundColor);
    }
}
