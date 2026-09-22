namespace Placard.Core.Theme;

internal static class PlacardTheme
{
    public static readonly Vector4 Transparent = new(0f, 0f, 0f, 0f);

    public static readonly Vector4 Accent = new(0.757f, 0.569f, 0.208f, 1f);
    public static readonly Vector4 AccentInk = new(0.110f, 0.086f, 0.031f, 1f);

    public static readonly Vector4 TitleInk = new(0.949f, 0.933f, 0.898f, 1f);
    public static readonly Vector4 BodyInk = new(0.847f, 0.827f, 0.784f, 1f);
    public static readonly Vector4 MutedInk = new(0.612f, 0.592f, 0.549f, 1f);
    public static readonly Vector4 HeaderInk = new(0.545f, 0.514f, 0.451f, 1f);

    public static readonly Vector4 Surface = new(0.086f, 0.086f, 0.082f, 1f);
    public static readonly Vector4 SurfaceMuted = new(0.129f, 0.125f, 0.118f, 1f);
    public static readonly Vector4 FieldSurface = new(1f, 1f, 1f, 0.055f);
    public static readonly Vector4 CardFill = new(1f, 1f, 1f, 0.035f);
    public static readonly Vector4 CardStroke = new(1f, 1f, 1f, 0.090f);
    public static readonly Vector4 Separator = new(1f, 1f, 1f, 0.100f);
    public static readonly Vector4 WindowEdge = new(1f, 1f, 1f, 0.170f);
    public static readonly Vector4 HoverTint = new(1f, 1f, 1f, 0.060f);
    public static readonly Vector4 ControlTrack = new(0.078f, 0.076f, 0.071f, 0.92f);
    public static readonly Vector4 ControlTrackHover = new(0.108f, 0.105f, 0.099f, 0.95f);
    public static readonly Vector4 MapPanel = new(0.072f, 0.070f, 0.066f, 1f);
    public static readonly Vector4 MapWash = new(0.040f, 0.038f, 0.035f, 0.22f);
    public static readonly Vector4 MarkerLabel = new(0.060f, 0.058f, 0.054f, 0.76f);

    public static readonly Vector4 SelectionHover = new(1f, 1f, 1f, 0.075f);
    public static readonly Vector4 SelectionFill = new(0.757f, 0.569f, 0.208f, 0.180f);
    public static readonly Vector4 SelectionStrongFill = Accent;
    public static readonly Vector4 SelectionInk = new(0.949f, 0.878f, 0.706f, 1f);
    public static readonly Vector4 FocusRing = new(0.757f, 0.569f, 0.208f, 0.680f);

    public static readonly Vector4 Danger = new(0.878f, 0.353f, 0.318f, 1f);

    public static readonly Vector4 GroupedCard = new(1f, 1f, 1f, 0.045f);
    public static readonly Vector4 Backdrop = new(0.058f, 0.056f, 0.052f, 1f);
    public static readonly Vector4 ToggleOff = new(1f, 1f, 1f, 0.160f);
    public static readonly Vector4 ToggleOn = Accent;

    public static readonly Vector4 Parchment = new(0.859f, 0.804f, 0.659f, 1f);
    public static readonly Vector4 Brass = new(0.746f, 0.531f, 0.114f, 1f);
    public static readonly Vector4 Results = new(0.883f, 0.453f, 0.114f, 1f);
    public static readonly Vector4 Closed = new(0.541f, 0.561f, 0.612f, 1f);
}
