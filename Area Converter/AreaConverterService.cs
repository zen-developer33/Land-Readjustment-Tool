using System.Globalization;

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
        private const double SqmPerSqft = 0.09290304;
        
        // Ropani system (used in hilly regions - Ropani, Aana, Paisa, Dam)
        private const double SqmPerRopani = 508.73704704;
        private const double SqmPerAana = 31.79606544;      // 1 Ropani = 16 Aana
        private const double SqmPerPaisa = 7.94901636;      // 1 Aana = 4 Paisa
        private const double SqmPerDam = 1.98725409;        // 1 Paisa = 4 Dam
        
        // Bigha system (used in Terai region - Bigha, Kattha, Dhur)
        private const double SqmPerBigha = 6772.631616;
        private const double SqmPerKattha = 338.6315808;   // 1 Bigha = 20 Kattha
        private const double SqmPerDhur = 16.93157904;     // 1 Kattha = 20 Dhur

        // Default precision rules
        private const int DefaultPrecision = 3;
        private const int SubUnitPrecision = 2;

        // Units per parent
        private const int AanaPerRopani = 16;
        private const int PaisaPerAana = 4;
        private const int DamPerPaisa = 4;
        private const int KatthaPerBigha = 20;
        private const int DhurPerKattha = 20;

        #endregion

        #region Square Meter Conversions

        public static double SqmToSqft(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerSqft, precision);

        public static double SqftToSqm(double sqft, int precision = DefaultPrecision)
            => RoundValue(sqft * SqmPerSqft, precision);

        public static double SqmToRopani(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerRopani, precision);

        public static double RopaniToSqm(double ropani, int precision = DefaultPrecision)
            => RoundValue(ropani * SqmPerRopani, precision);

        public static double SqmToAana(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerAana, precision);

        public static double AanaToSqm(double aana, int precision = DefaultPrecision)
            => RoundValue(aana * SqmPerAana, precision);

        public static double SqmToPaisa(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerPaisa, precision);

        public static double PaisaToSqm(double paisa, int precision = DefaultPrecision)
            => RoundValue(paisa * SqmPerPaisa, precision);

        public static double SqmToDam(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerDam, precision);

        public static double DamToSqm(double dam, int precision = DefaultPrecision)
            => RoundValue(dam * SqmPerDam, precision);

        public static double SqmToBigha(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerBigha, precision);

        public static double BighaToSqm(double bigha, int precision = DefaultPrecision)
            => RoundValue(bigha * SqmPerBigha, precision);

        public static double SqmToKattha(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerKattha, precision);

        public static double KatthaToSqm(double kattha, int precision = DefaultPrecision)
            => RoundValue(kattha * SqmPerKattha, precision);

        public static double SqmToDhur(double sqm, int precision = DefaultPrecision)
            => RoundValue(sqm / SqmPerDhur, precision);

        public static double DhurToSqm(double dhur, int precision = DefaultPrecision)
            => RoundValue(dhur * SqmPerDhur, precision);

        #endregion

        #region RAPD (Ropani-Aana-Paisa-Dam) Conversions

        /// <summary>
        /// Converts square meters to RAPD components
        /// </summary>
        public static (int Ropani, int Aana, int Paisa, double Dam) SqmToRAPDComponents(
            double sqm,
            int damPrecision = SubUnitPrecision)
        {
            double remaining = Math.Max(0, sqm);

            int ropani = (int)(remaining / SqmPerRopani);
            remaining -= ropani * SqmPerRopani;

            int aana = (int)(remaining / SqmPerAana);
            remaining -= aana * SqmPerAana;

            int paisa = (int)(remaining / SqmPerPaisa);
            remaining -= paisa * SqmPerPaisa;

            double dam = RoundSubUnit(remaining / SqmPerDam, damPrecision);

            // Handle overflow
            if (dam >= DamPerPaisa) { dam = 0; paisa++; }
            if (paisa >= PaisaPerAana) { paisa = 0; aana++; }
            if (aana >= AanaPerRopani) { aana = 0; ropani++; }

            return (ropani, aana, paisa, dam);
        }

        /// <summary>
        /// Converts RAPD components to square meters
        /// </summary>
        public static double RAPDToSqm(int ropani, int aana, int paisa, double dam, int precision = DefaultPrecision)
        {
            var sqm = (ropani * SqmPerRopani) +
                      (aana * SqmPerAana) +
                      (paisa * SqmPerPaisa) +
                      (dam * SqmPerDam);

            return RoundValue(sqm, precision);
        }

        /// <summary>
        /// Converts RAPD string (format: "R-A-P-D" or "1-2-3-4") to square meters
        /// </summary>
        public static double? ParseRAPDToSqm(string? rapd, int precision = DefaultPrecision)
        {
            if (string.IsNullOrWhiteSpace(rapd)) return null;

            try
            {
                var parts = rapd.Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) return null;

                int ropani = parts.Length > 0 ? ParseInt(parts[0]) : 0;
                int aana = parts.Length > 1 ? ParseInt(parts[1]) : 0;
                int paisa = parts.Length > 2 ? ParseInt(parts[2]) : 0;
                double dam = parts.Length > 3 ? ParseDouble(parts[3]) : 0;

                return RAPDToSqm(ropani, aana, paisa, dam, precision);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Formats RAPD components to string
        /// </summary>
        public static string FormatRAPD(
            int ropani,
            int aana,
            int paisa,
            double dam,
            int damPrecision = SubUnitPrecision)
        {
            int normalizedDamPrecision = NormalizePrecision(damPrecision);
            return $"{ropani}-{aana}-{paisa}-{RoundSubUnit(dam, normalizedDamPrecision).ToString($"F{normalizedDamPrecision}", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Converts square meters to RAPD formatted string
        /// </summary>
        public static string SqmToRAPDString(
            double sqm,
            int damPrecision = SubUnitPrecision)
        {
            var (r, a, p, d) = SqmToRAPDComponents(sqm, damPrecision);
            return FormatRAPD(r, a, p, d, damPrecision);
        }

        #endregion

        #region BKD (Bigha-Kattha-Dhur) Conversions

        /// <summary>
        /// Converts square meters to BKD components
        /// </summary>
        public static (int Bigha, int Kattha, double Dhur) SqmToBKDComponents(
            double sqm,
            int dhurPrecision = SubUnitPrecision)
        {
            double remaining = Math.Max(0, sqm);

            int bigha = (int)(remaining / SqmPerBigha);
            remaining -= bigha * SqmPerBigha;

            int kattha = (int)(remaining / SqmPerKattha);
            remaining -= kattha * SqmPerKattha;

            double dhur = RoundSubUnit(remaining / SqmPerDhur, dhurPrecision);

            // Handle overflow
            if (dhur >= DhurPerKattha) { dhur = 0; kattha++; }
            if (kattha >= KatthaPerBigha) { kattha = 0; bigha++; }

            return (bigha, kattha, dhur);
        }

        /// <summary>
        /// Converts BKD components to square meters
        /// </summary>
        public static double BKDToSqm(int bigha, int kattha, double dhur, int precision = DefaultPrecision)
        {
            var sqm = (bigha * SqmPerBigha) +
                      (kattha * SqmPerKattha) +
                      (dhur * SqmPerDhur);

            return RoundValue(sqm, precision);
        }

        /// <summary>
        /// Converts BKD string (format: "B-K-D" or "1-2-3") to square meters
        /// </summary>
        public static double? ParseBKDToSqm(string? bkd, int precision = DefaultPrecision)
        {
            if (string.IsNullOrWhiteSpace(bkd)) return null;

            try
            {
                var parts = bkd.Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) return null;

                int bigha = parts.Length > 0 ? ParseInt(parts[0]) : 0;
                int kattha = parts.Length > 1 ? ParseInt(parts[1]) : 0;
                double dhur = parts.Length > 2 ? ParseDouble(parts[2]) : 0;

                return BKDToSqm(bigha, kattha, dhur, precision);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Formats BKD components to string
        /// </summary>
        public static string FormatBKD(
            int bigha,
            int kattha,
            double dhur,
            int dhurPrecision = SubUnitPrecision)
        {
            int normalizedDhurPrecision = NormalizePrecision(dhurPrecision);
            return $"{bigha}-{kattha}-{RoundSubUnit(dhur, normalizedDhurPrecision).ToString($"F{normalizedDhurPrecision}", CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Converts square meters to BKD formatted string
        /// </summary>
        public static string SqmToBKDString(
            double sqm,
            int dhurPrecision = SubUnitPrecision)
        {
            var (b, k, d) = SqmToBKDComponents(sqm, dhurPrecision);
            return FormatBKD(b, k, d, dhurPrecision);
        }

        #endregion

        #region Generic Ropani Conversion (for filtering)

        /// <summary>
        /// Converts Ropani value (decimal) to square meters for filtering
        /// This treats the input as pure Ropani units
        /// </summary>
        public static double RopaniDecimalToSqm(double ropaniDecimal, int precision = DefaultPrecision)
        {
            return RoundValue(ropaniDecimal * SqmPerRopani, precision);
        }

        /// <summary>
        /// Gets area in square meters from a parcel, trying AreaInSqm first, 
        /// then parsing RAPD or BKD if needed
        /// </summary>
        public static double? GetAreaInSqm(double? areaInSqm, string? areaInRAPD, string? areaInBKD, int precision = DefaultPrecision)
        {
            if (areaInSqm.HasValue && areaInSqm.Value > 0)
                return RoundValue(areaInSqm.Value, precision);

            var rapdSqm = ParseRAPDToSqm(areaInRAPD, precision);
            if (rapdSqm.HasValue && rapdSqm.Value > 0)
                return rapdSqm.Value;

            var bkdSqm = ParseBKDToSqm(areaInBKD, precision);
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

        private static double ParseDouble(string value)
        {
            var normalized = value.Trim();

            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantResult))
                return invariantResult;

            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out var currentCultureResult))
                return currentCultureResult;

            return 0;
        }

        private static double RoundValue(double value, int precision)
        {
            return Math.Round(value, NormalizePrecision(precision), MidpointRounding.AwayFromZero);
        }

        private static double RoundSubUnit(double value, int precision = SubUnitPrecision)
        {
            return Math.Round(value, NormalizePrecision(precision), MidpointRounding.AwayFromZero);
        }

        private static int NormalizePrecision(int precision)
        {
            if (precision < 0)
                return 0;

            return precision;
        }

        #endregion
    }
}
