using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Components;

internal static class ScrollLayout
{
    public static float StableContentWidth()
    {
        var available = ImGui.GetContentRegionAvail().X;
        return ImGui.GetScrollMaxY() > 0f ? available : available - ImGui.GetStyle().ScrollbarSize;
    }
}
