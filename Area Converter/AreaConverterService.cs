namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Service for converting between Nepal land measurement units.
    /// Supports: Square Meters (sqm), Square Feet (sqft), Ropani-Aana-Paisa-Dam (RAPD), Bigha-Kattha-Dhur (BKD)
    /// </summary>
    public static class AreaConverterService
    {
        #region Conversion Constants

        // Base unit: Square Meters (sqm)
        private const double SqmPerSqft = 0.092903;
        
        // Ropani system (used in hilly regions - Ropani, Aana, Paisa, Dam)
        private const double SqmPerRopani = 508.74;
        private const double SqmPerAana = 31.80;      // 1 Ropani = 16 Aana
        private const double SqmPerPaisa = 7.95;      // 1 Aana = 4 Paisa
        private const double SqmPerDam = 1.99;        // 1 Paisa = 4 Dam
        
        // Bigha system (used in Terai region - Bigha, Kattha, Dhur)
        private const double SqmPerBigha = 6772.63;
        private const double SqmPerKattha = 338.63;   // 1 Bigha = 20 Kattha
        private const double SqmPerDhur = 16.93;      // 1 Kattha = 20 Dhur

        // Units per parent
        private const int AanaPerRopani = 16;
        private const int PaisaPerAana = 4;
        private const int DamPerPaisa = 4;
        private const int KatthaPerBigha = 20;
        private const int DhurPerKattha = 20;

        #endregion

        #region Square Meter Conversions

        public static double SqmToSqft(double sqm) => sqm / SqmPerSqft;
        public static double SqftToSqm(double sqft) => sqft * SqmPerSqft;
        public static double SqmToRopani(double sqm) => sqm / SqmPerRopani;
        public static double RopaniToSqm(double ropani) => ropani * SqmPerRopani;
        public static double SqmToBigha(double sqm) => sqm / SqmPerBigha;
        public static double BighaToSqm(double bigha) => bigha * SqmPerBigha;

        #endregion

        #region RAPD (Ropani-Aana-Paisa-Dam) Conversions

        /// <summary>
        /// Converts square meters to RAPD components
        /// </summary>
        public static (int Ropani, int Aana, int Paisa, int Dam) SqmToRAPDComponents(double sqm)
        {
            double remaining = sqm;

            int ropani = (int)(remaining / SqmPerRopani);
            remaining -= ropani * SqmPerRopani;

            int aana = (int)(remaining / SqmPerAana);
            remaining -= aana * SqmPerAana;

            int paisa = (int)(remaining / SqmPerPaisa);
            remaining -= paisa * SqmPerPaisa;

            int dam = (int)Math.Round(remaining / SqmPerDam);

            // Handle overflow
            if (dam >= DamPerPaisa) { dam = 0; paisa++; }
            if (paisa >= PaisaPerAana) { paisa = 0; aana++; }
            if (aana >= AanaPerRopani) { aana = 0; ropani++; }

            return (ropani, aana, paisa, dam);
        }

        /// <summary>
        /// Converts RAPD components to square meters
        /// </summary>
        public static double RAPDToSqm(int ropani, int aana, int paisa, int dam)
        {
            return (ropani * SqmPerRopani) + 
                   (aana * SqmPerAana) + 
                   (paisa * SqmPerPaisa) + 
                   (dam * SqmPerDam);
        }

        /// <summary>
        /// Converts RAPD string (format: "R-A-P-D" or "1-2-3-4") to square meters
        /// </summary>
        public static double? ParseRAPDToSqm(string? rapd)
        {
            if (string.IsNullOrWhiteSpace(rapd)) return null;

            try
            {
                var parts = rapd.Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) return null;

                int ropani = parts.Length > 0 ? ParseInt(parts[0]) : 0;
                int aana = parts.Length > 1 ? ParseInt(parts[1]) : 0;
                int paisa = parts.Length > 2 ? ParseInt(parts[2]) : 0;
                int dam = parts.Length > 3 ? ParseInt(parts[3]) : 0;

                return RAPDToSqm(ropani, aana, paisa, dam);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Formats RAPD components to string
        /// </summary>
        public static string FormatRAPD(int ropani, int aana, int paisa, int dam)
        {
            return $"{ropani}-{aana}-{paisa}-{dam}";
        }

        /// <summary>
        /// Converts square meters to RAPD formatted string
        /// </summary>
        public static string SqmToRAPDString(double sqm)
        {
            var (r, a, p, d) = SqmToRAPDComponents(sqm);
            return FormatRAPD(r, a, p, d);
        }

        #endregion

        #region BKD (Bigha-Kattha-Dhur) Conversions

        /// <summary>
        /// Converts square meters to BKD components
        /// </summary>
        public static (int Bigha, int Kattha, int Dhur) SqmToBKDComponents(double sqm)
        {
            double remaining = sqm;

            int bigha = (int)(remaining / SqmPerBigha);
            remaining -= bigha * SqmPerBigha;

            int kattha = (int)(remaining / SqmPerKattha);
            remaining -= kattha * SqmPerKattha;

            int dhur = (int)Math.Round(remaining / SqmPerDhur);

            // Handle overflow
            if (dhur >= DhurPerKattha) { dhur = 0; kattha++; }
            if (kattha >= KatthaPerBigha) { kattha = 0; bigha++; }

            return (bigha, kattha, dhur);
        }

        /// <summary>
        /// Converts BKD components to square meters
        /// </summary>
        public static double BKDToSqm(int bigha, int kattha, int dhur)
        {
            return (bigha * SqmPerBigha) + 
                   (kattha * SqmPerKattha) + 
                   (dhur * SqmPerDhur);
        }

        /// <summary>
        /// Converts BKD string (format: "B-K-D" or "1-2-3") to square meters
        /// </summary>
        public static double? ParseBKDToSqm(string? bkd)
        {
            if (string.IsNullOrWhiteSpace(bkd)) return null;

            try
            {
                var parts = bkd.Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) return null;

                int bigha = parts.Length > 0 ? ParseInt(parts[0]) : 0;
                int kattha = parts.Length > 1 ? ParseInt(parts[1]) : 0;
                int dhur = parts.Length > 2 ? ParseInt(parts[2]) : 0;

                return BKDToSqm(bigha, kattha, dhur);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Formats BKD components to string
        /// </summary>
        public static string FormatBKD(int bigha, int kattha, int dhur)
        {
            return $"{bigha}-{kattha}-{dhur}";
        }

        /// <summary>
        /// Converts square meters to BKD formatted string
        /// </summary>
        public static string SqmToBKDString(double sqm)
        {
            var (b, k, d) = SqmToBKDComponents(sqm);
            return FormatBKD(b, k, d);
        }

        #endregion

        #region Generic Ropani Conversion (for filtering)

        /// <summary>
        /// Converts Ropani value (decimal) to square meters for filtering
        /// This treats the input as pure Ropani units
        /// </summary>
        public static double RopaniDecimalToSqm(double ropaniDecimal)
        {
            return ropaniDecimal * SqmPerRopani;
        }

        /// <summary>
        /// Gets area in square meters from a parcel, trying AreaInSqm first, 
        /// then parsing RAPD or BKD if needed
        /// </summary>
        public static double? GetAreaInSqm(double? areaInSqm, string? areaInRAPD, string? areaInBKD)
        {
            if (areaInSqm.HasValue && areaInSqm.Value > 0)
                return areaInSqm.Value;

            var rapdSqm = ParseRAPDToSqm(areaInRAPD);
            if (rapdSqm.HasValue && rapdSqm.Value > 0)
                return rapdSqm.Value;

            var bkdSqm = ParseBKDToSqm(areaInBKD);
            if (bkdSqm.HasValue && bkdSqm.Value > 0)
                return bkdSqm.Value;

            return null;
        }

        #endregion

        #region Helpers

        private static int ParseInt(string value)
        {
            return int.TryParse(value.Trim(), out int result) ? result : 0;
        }

        #endregion
    }
}
