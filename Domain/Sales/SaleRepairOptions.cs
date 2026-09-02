namespace APISales.Domain.Sales
{
    public static class SaleRepairOptions
    {
        public static readonly string[] AudienceTypes = { "Adult", "Child" };
        public static readonly string[] ActionTypes = { "Adjustment", "Replacement", "Addition", "Removal" };

        public static bool IsValidAudienceType(string? value)
            => AudienceTypes.Contains((value ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase);

        public static bool IsValidActionType(string? value)
            => ActionTypes.Contains((value ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase);

        public static string NormalizeAudienceType(string? value)
            => Normalize(value, AudienceTypes, "Adult");

        public static string NormalizeActionType(string? value)
            => Normalize(value, ActionTypes, "Adjustment");

        private static string Normalize(string? value, string[] allowedValues, string fallback)
        {
            var trimmed = (value ?? string.Empty).Trim();
            return allowedValues.FirstOrDefault(v => string.Equals(v, trimmed, StringComparison.OrdinalIgnoreCase)) ?? fallback;
        }
    }
}
