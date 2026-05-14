using System.Globalization;
using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Helpers
{
    internal static class ColorDialogCustomColorsStore
    {
        private const int MaxCustomColors = 16;

        public static readonly (string Name, Color Color)[] ArcMapPalette =
        [
            ("Green", Color.FromArgb(107, 174, 110)),
            ("Blue", Color.FromArgb(142, 211, 230)),
            ("Sun", Color.FromArgb(255, 255, 102)),
            ("Hollow", Color.White),
            ("Lake", Color.FromArgb(149, 212, 232)),
            ("Rose", Color.FromArgb(246, 179, 182)),
            ("Beige", Color.FromArgb(246, 227, 180)),
            ("Yellow", Color.FromArgb(255, 255, 190)),
            ("Olive", Color.FromArgb(223, 245, 184)),
            ("Light Green", Color.FromArgb(207, 246, 194)),
            ("Jade", Color.FromArgb(184, 240, 223)),
            ("Light Blue", Color.FromArgb(183, 221, 240)),
            ("Med Blue", Color.FromArgb(183, 201, 239)),
            ("Lilac", Color.FromArgb(217, 169, 240)),
            ("Violet", Color.FromArgb(241, 169, 216)),
            ("Grey", Color.FromArgb(217, 217, 217)),
            ("Orange", Color.FromArgb(217, 154, 90)),
            ("Coral", Color.FromArgb(207, 124, 130)),
            ("Pink", Color.FromArgb(228, 162, 195)),
            ("Tan", Color.FromArgb(233, 223, 184)),
            ("Lt Orange", Color.FromArgb(246, 199, 102)),
            ("Med Green", Color.FromArgb(167, 232, 174)),
            ("Med Yellow", Color.FromArgb(238, 240, 155)),
            ("Flood Overlay", Color.FromArgb(191, 203, 221))
        ];

        private static readonly string StoreFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Land_Readjustment_Tool",
            "color-dialog-custom-colors.txt");

        public static void LoadInto(ColorDialog dialog)
        {
            if (dialog == null) return;

            var colors = GetDefaultCustomColors()
                .Concat(LoadColors())
                .Distinct()
                .Take(MaxCustomColors)
                .ToArray();
            if (colors.Length > 0)
                dialog.CustomColors = colors;
        }

        public static void SaveFrom(ColorDialog dialog)
        {
            if (dialog?.CustomColors == null) return;
            SaveColors(dialog.CustomColors);
        }

        public static Color[] GetLayerPaletteColors()
        {
            return ArcMapPalette
                .Select(entry => entry.Color)
                .Concat(LoadColors().Select(ColorTranslator.FromOle))
                .Where(color => color.A > 0)
                .GroupBy(color => Color.FromArgb(255, color.R, color.G, color.B).ToArgb())
                .Select(group => group.First())
                .ToArray();
        }

        private static int[] LoadColors()
        {
            try
            {
                if (!File.Exists(StoreFilePath))
                    return [];

                var text = File.ReadAllText(StoreFilePath).Trim();
                if (string.IsNullOrWhiteSpace(text))
                    return [];

                var values = text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(token =>
                        int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                            ? value
                            : -1)
                    .Where(value => value >= 0 && value <= 0x00FFFFFF)
                    .Take(MaxCustomColors)
                    .ToArray();

                return values;
            }
            catch
            {
                return [];
            }
        }

        private static int[] GetDefaultCustomColors()
        {
            return ArcMapPalette
                .Select(entry => ColorTranslator.ToOle(entry.Color))
                .Take(MaxCustomColors)
                .ToArray();
        }

        private static void SaveColors(int[] colors)
        {
            try
            {
                var normalized = colors
                    .Where(value => value >= 0 && value <= 0x00FFFFFF)
                    .Take(MaxCustomColors)
                    .ToArray();

                Directory.CreateDirectory(Path.GetDirectoryName(StoreFilePath)!);
                File.WriteAllText(
                    StoreFilePath,
                    string.Join(",", normalized.Select(v => v.ToString(CultureInfo.InvariantCulture))));
            }
            catch
            {
                // Best-effort persistence. Ignore I/O failures.
            }
        }
    }
}
