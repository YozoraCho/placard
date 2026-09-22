using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class EmptyState
{
    private const float IconRadius = 34f;
    private const float IconGlyphScale = 1.7f;
    private const float TitleGap = 58f;
    private const float HintGap = 84f;
    private const float SideInset = 56f;
    private const float HintMaxWidth = 300f;
    private const float CenterLift = 40f;

    public static void Draw(Rect body, FontAwesomeIcon icon, string title, string hint)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var centerX = body.Center.X;
        var baseY = body.Center.Y - CenterLift * scale;
        var iconCenter = new Vector2(centerX, baseY);
        drawList.AddCircleFilled(iconCenter, IconRadius * scale, ImGui.GetColorU32(PlacardTheme.FieldSurface), 32);
        Icons.DrawCentered(drawList, iconCenter, icon, PlacardTheme.MutedInk, IconGlyphScale);

        var maxWidth = MathF.Max(1f, body.Width - SideInset * scale);
        var titleDisplay = Typography.FitText(title, maxWidth, TextStyles.Title3);
        Typography.DrawCentered(drawList, new Vector2(centerX, baseY + TitleGap * scale), titleDisplay,
            PlacardTheme.TitleInk, TextStyles.Title3);
        if (hint.Length == 0)
        {
            return;
        }

        var hintWidth = MathF.Min(maxWidth, HintMaxWidth * scale);
        Typography.DrawWrappedCentered(drawList, hint, TextStyles.Subheadline, PlacardTheme.MutedInk,
            new Vector2(centerX, baseY + HintGap * scale), hintWidth);
    }
}
