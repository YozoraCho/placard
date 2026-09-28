using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;

namespace Placard.Windows.Housing;

internal static class HousingPoiMarkers
{
    private const float IconSize = 20f;

    public static void Draw(
        ImDrawListPtr drawList,
        Vector2 center,
        uint iconId,
        float scale)
    {
        if (iconId == 0)
        {
            return;
        }

        var lookup = new GameIconLookup
        {
            IconId = iconId,
        };

        var texture = global::Placard.Plugin.TextureProvider
            .GetFromGameIcon(lookup)
            .GetWrapOrDefault();

        if (texture is null)
        {
            return;
        }

        var size = IconSize * scale;
        var half = size * 0.5f;

        var min = new Vector2(
            center.X - half,
            center.Y - half);

        var max = new Vector2(
            center.X + half,
            center.Y + half);

        drawList.AddImage(
            texture.Handle,
            min,
            max,
            Vector2.Zero,
            Vector2.One,
            0xFFFFFFFF);
    }
}
