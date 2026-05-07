namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Adds plain-language guidance to tile server errors while keeping the
    /// original diagnostic message visible.
    /// </summary>
    public static class XyzTileErrorMessageBuilder
    {
        public static string AddUserGuidance(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;

            if (LooksLikeAccessDenied(message))
            {
                return message +
                    Environment.NewLine +
                    Environment.NewLine +
                    "This tile server refused access to the imagery. The source may require an API key, sign-in, billing, a valid license, or permission from the map provider. Choose a built-in public source such as Bing or OpenStreetMap, or update the URL with the required authentication details.";
            }

            if (LooksLikeRateLimited(message))
            {
                return message +
                    Environment.NewLine +
                    Environment.NewLine +
                    "The tile server is temporarily limiting requests. Try again later, reduce the selected area or zoom level, or use a source that allows larger tile downloads.";
            }

            return message;
        }

        private static bool LooksLikeAccessDenied(string message)
        {
            return message.Contains("401", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("403", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("407", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("forbidden", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("access denied", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("permission", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("api key", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("billing", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("payment required", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeRateLimited(string message)
        {
            return message.Contains("429", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("too many requests", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("rate-limit", StringComparison.OrdinalIgnoreCase);
        }
    }
}
