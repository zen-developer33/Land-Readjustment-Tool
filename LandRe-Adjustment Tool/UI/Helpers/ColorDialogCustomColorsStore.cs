using System.Globalization;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Helpers
{
    internal static class ColorDialogCustomColorsStore
    {
        private const int MaxCustomColors = 16;

        private static readonly string StoreFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Land_Readjustment_Tool",
            "color-dialog-custom-colors.txt");

        public static void LoadInto(ColorDialog dialog)
        {
            if (dialog == null) return;

            var colors = LoadColors();
            if (colors.Length > 0)
                dialog.CustomColors = colors;
        }

        public static void SaveFrom(ColorDialog dialog)
        {
            if (dialog?.CustomColors == null) return;
            SaveColors(dialog.CustomColors);
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
