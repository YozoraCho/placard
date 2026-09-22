namespace Placard.Core.Animation;

internal static class Pulse
{
    public const double Fast = 600.0;
    public const double Medium = 800.0;
    public const double Breath = 2600.0;
    public const double Calm = 1900.0;

    public static float Wave(double periodMs = Medium)
    {
        var position = Environment.TickCount % periodMs / periodMs;
        return (float)((Math.Sin(position * Math.PI * 2.0) + 1.0) * 0.5);
    }

    public static float Phase(double periodMs) => (float)(Environment.TickCount % periodMs / periodMs);

    public static Vector4 Blend(Vector4 from, Vector4 to, double periodMs = Medium) =>
        Vector4.Lerp(from, to, Wave(periodMs));
}
