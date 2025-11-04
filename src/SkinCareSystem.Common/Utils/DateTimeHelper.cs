using System;

namespace SkinCareSystem.Common.Utils
{
    /// <summary>
    /// Utility helpers for handling DateTime values when persisting to PostgreSQL.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Gets the current UTC timestamp with <see cref="DateTimeKind.Unspecified"/> for compatibility with timestamp without time zone columns.
        /// </summary>
        public static DateTime UtcNowUnspecified() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        /// <summary>
        /// Ensures the provided value is marked as <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>
        public static DateTime EnsureUnspecified(DateTime value) =>
            value.Kind == DateTimeKind.Unspecified
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

        /// <summary>
        /// Ensures the provided value is returned as UTC (defaulting to now when null).
        /// </summary>
        public static DateTime EnsureUtc(DateTime? value) =>
            value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }
}
