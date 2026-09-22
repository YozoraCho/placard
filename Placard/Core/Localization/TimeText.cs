namespace Placard.Core.Localization;

internal static class TimeText
{
    private const string Pattern24Hour = "HH:mm";
    private const string Pattern12Hour = "h:mm tt";

    private static bool use24Hour = true;

    public static string ClockPattern => use24Hour ? Pattern24Hour : Pattern12Hour;

    public static void ApplyClockPreference(bool? preference)
    {
        use24Hour = preference ?? CultureUses24Hour();
    }

    public static string Clock(DateTime moment) => moment.ToString(ClockPattern, Loc.Culture);

    public static string Clock(DateTimeOffset moment) => moment.ToString(ClockPattern, Loc.Culture);

    public static string Clock(long unixSeconds) =>
        unixSeconds <= 0L ? string.Empty : Clock(DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime());

    private static bool CultureUses24Hour() => Loc.Culture.DateTimeFormat.ShortTimePattern.Contains('H');
}
