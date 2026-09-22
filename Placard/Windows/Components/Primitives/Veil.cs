using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Components;

internal static class Veil
{
    public static void Draw(ImDrawListPtr drawList, Vector2 min, Vector2 max, float dim, float rounding = 0f)
    {
        if (dim <= 0f)
        {
            return;
        }

        var color = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, Math.Clamp(dim, 0f, 1f)));
        if (rounding <= 0f)
        {
            drawList.AddRectFilled(min, max, color);
            return;
        }

        Squircle.Fill(drawList, min, max, rounding, color);
    }
}
