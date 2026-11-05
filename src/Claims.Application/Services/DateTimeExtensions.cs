namespace Claims.Application.Services;

internal static class DateTimeExtensions
{
    /// <summary>
    /// Reads a date without an offset as UTC. Left as it is, the MongoDB driver would treat it as
    /// local time and shift it, which can move a cover to the previous day.
    /// </summary>
    public static DateTime AsUtc(this DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
}
