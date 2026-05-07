using System.Text;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Converts between XYZ tile coordinates and Bing Maps quadkey strings.
    /// </summary>
    public static class QuadkeyConverter
    {
        public static string TileXYToQuadkey(int tileX, int tileY, int zoomLevel)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(zoomLevel);

            StringBuilder quadkey = new(zoomLevel);
            for (int i = zoomLevel; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);

                if ((tileX & mask) != 0)
                    digit++;

                if ((tileY & mask) != 0)
                    digit = (char)(digit + 2);

                quadkey.Append(digit);
            }

            return quadkey.ToString();
        }

        public static (int TileX, int TileY, int ZoomLevel) QuadkeyToTileXY(
            string quadkey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(quadkey);

            int tileX = 0;
            int tileY = 0;
            int zoomLevel = quadkey.Length;

            for (int i = zoomLevel; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                char digit = quadkey[zoomLevel - i];

                switch (digit)
                {
                    case '0':
                        break;
                    case '1':
                        tileX |= mask;
                        break;
                    case '2':
                        tileY |= mask;
                        break;
                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;
                    default:
                        throw new ArgumentException(
                            "Quadkey must contain only digits 0, 1, 2, or 3.",
                            nameof(quadkey));
                }
            }

            return (tileX, tileY, zoomLevel);
        }
    }
}
