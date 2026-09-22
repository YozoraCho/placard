using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal sealed class ConfirmRequest
{
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required string ConfirmLabel { get; init; }
    public required string CancelLabel { get; init; }
    public required Action Confirm { get; init; }
    public bool Destructive { get; init; }

    public bool Sheet { get; init; }
}

internal sealed class ConfirmPrompt
{
    private const float CardWidth = 320f;
    private const float Padding = 20f;
    private const float ButtonHeight = 34f;
    private const float ButtonGap = 8f;

    private ConfirmRequest? pending;

    public bool IsOpen => pending is not null;

    public void Ask(ConfirmRequest request)
    {
        pending = request;
    }

    public void Dismiss()
    {
        pending = null;
    }

    public void Draw(Rect area)
    {
        if (pending is not { } request)
        {
            return;
        }

        var scale = UiScale.Current;
        var drawList = ImGui.GetForegroundDrawList();
        Veil.Draw(drawList, area.Min, area.Max, 0.45f);
        UiInteract.BlockThisFrame();

        var padding = Padding * scale;
        var width = MathF.Min(CardWidth * scale, MathF.Max(1f, area.Width - padding * 2f));
        var textWidth = MathF.Max(1f, width - padding * 2f);
        var titleHeight = Typography.LineHeight(TextStyles.Title3);
        var messageHeight = Typography.MeasureWrappedBlock(request.Message, TextStyles.Subheadline, textWidth).Y;
        var buttonHeight = ButtonHeight * scale;
        var height = padding * 2f + titleHeight + Metrics.Space.Sm * scale + messageHeight
            + Metrics.Space.Lg * scale + buttonHeight;
        var min = new Vector2(area.Center.X - width * 0.5f, area.Center.Y - height * 0.5f);
        var max = min + new Vector2(width, height);
        var card = new Rect(min, max);
        UiInteract.HoverOverlay(card);

        var radius = Metrics.Radius.Card * scale;
        Squircle.Fill(drawList, min, max, radius, ImGui.GetColorU32(PlacardTheme.SurfaceMuted));
        Squircle.Stroke(drawList, min, max, radius, ImGui.GetColorU32(PlacardTheme.CardStroke),
            Metrics.Stroke.Hairline * scale);

        var left = min.X + padding;
        var y = min.Y + padding;
        Typography.Draw(drawList, new Vector2(left, y),
            Typography.FitText(request.Title, textWidth, TextStyles.Title3), PlacardTheme.TitleInk,
            TextStyles.Title3);
        y += titleHeight + Metrics.Space.Sm * scale;
        Typography.DrawWrappedLeft(drawList, new Vector2(left, y), request.Message, PlacardTheme.MutedInk,
            TextStyles.Subheadline, textWidth);
        y += messageHeight + Metrics.Space.Lg * scale;

        var gap = ButtonGap * scale;
        var buttonWidth = (textWidth - gap) * 0.5f;
        var cancelRect = new Rect(new Vector2(left, y), new Vector2(left + buttonWidth, y + buttonHeight));
        var confirmRect = new Rect(new Vector2(left + buttonWidth + gap, y),
            new Vector2(left + textWidth, y + buttonHeight));

        if (PillButton.Draw(cancelRect, request.CancelLabel, false, PlacardTheme.Accent, true))
        {
            pending = null;
            return;
        }

        var accent = request.Destructive ? PlacardTheme.Danger : PlacardTheme.Accent;
        if (PillButton.Draw(confirmRect, request.ConfirmLabel, true, accent, true))
        {
            var confirm = request.Confirm;
            pending = null;
            confirm();
        }
    }
}
