using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Components;

internal static class Elevation
{
    private const int Layers = 6;
    private const float HorizontalBias = 0.12f;

    public static void Card(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float scale,
        float opacity = 1f) =>
        Draw(drawList, min, max, rounding, scale, 6f, 2f, 0.10f, opacity);

    public static void Floating(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float scale,
        float opacity = 1f) =>
        Draw(drawList, min, max, rounding, scale, 10f, 3f, 0.14f, opacity);

    public static void Icon(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float radius,
        float opacity = 1f) =>
        Draw(drawList, min, max, rounding, 1f, radius * 0.22f, radius * 0.10f, 0.14f, opacity);

    public static void Draw(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float scale,
        float spread, float yOffset, float strength, float opacity = 1f, int layers = Layers)
    {
        if (opacity <= 0f || strength <= 0f)
        {
            return;
        }

        var maxSpread = spread * scale;
        var drop = new Vector2(-yOffset * scale * HorizontalBias, yOffset * scale);
        var layerColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, strength / layers * opacity));
        for (var index = 0; index < layers; index++)
        {
            var inset = maxSpread * (1f - index / (float)layers);
            var layerMin = min - new Vector2(inset, inset) + drop;
            var layerMax = max + new Vector2(inset, inset) + drop;
            drawList.AddRectFilled(layerMin, layerMax, layerColor, rounding + inset);
        }
    }
}
