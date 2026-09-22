using Dalamud.Interface.Utility;

namespace Placard.Core;

internal static class UiScale
{
    private const float MinimumZoom = 0.75f;
    private const float MaximumZoom = 2f;

    public static float Zoom { get; private set; } = 1f;

    public static float Current => ImGuiHelpers.GlobalScale * Zoom;

    public static float Global => ImGuiHelpers.GlobalScale;

    public static void SetZoom(float zoom)
    {
        Zoom = Math.Clamp(zoom, MinimumZoom, MaximumZoom);
    }
}
