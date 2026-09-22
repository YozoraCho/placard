using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Placard.Core;

namespace Placard.Windows.Components;

internal static class AppSurface
{
    public const float SidePadding = 16f;

    public static SurfaceScope Begin(Rect area) => Begin(area, SidePadding);

    public static SurfaceScope BeginEdgeToEdge(Rect area) => Begin(area, 0f);

    public static SurfaceScope BeginOverlay(Rect area)
    {
        ImGui.SetCursorScreenPos(area.Min);
        var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        var child = ImRaii.Child("##placardOverlay", area.Size, false,
            ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        return new SurfaceScope(child, padding);
    }

    public static SurfaceScope Begin(Rect area, float sidePadding)
    {
        var scale = UiScale.Current;
        ImGui.SetCursorScreenPos(area.Min);
        var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding,
            new Vector2(sidePadding * scale, Metrics.Space.Sm * scale));
        var child = ImRaii.Child("##placardSurface", area.Size, false, ImGuiWindowFlags.NoBackground);
        return new SurfaceScope(child, padding);
    }

    public ref struct SurfaceScope
    {
        private ImRaii.ChildDisposable child;
        private readonly IDisposable padding;

        internal SurfaceScope(ImRaii.ChildDisposable child, IDisposable padding)
        {
            this.child = child;
            this.padding = padding;
        }

        public void Dispose()
        {
            child.Dispose();
            padding?.Dispose();
        }
    }
}
