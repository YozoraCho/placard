using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Placard.Core;
using Placard.Core.Housing;
using Placard.Core.Theme;
using Placard.Windows.Components;
using Placard.Windows.Housing;

namespace Placard.Windows;

internal sealed class PlacardWindow : Window, IDisposable
{
    private const ImGuiWindowFlags BaseFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar |
                                               ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse |
                                               ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove;

    public const float FrameInset = 5f;

    private static readonly Vector2 DefaultSize = new(1020f, 700f);
    private static readonly Vector2 MinimumSize = new(600f, 460f);

    private readonly HousingService housing;
    private readonly HousingApp app;

    private bool dragging;

    public PlacardWindow(Configuration configuration, HousingService housing)
        : base("Placard##placard.main.v2", BaseFlags)
    {
        this.housing = housing;
        app = new HousingApp(housing, configuration, () => IsOpen = false);
        Size = DefaultSize;
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = MinimumSize,
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void OnOpen()
    {
        housing.EnsureStarted();
        housing.EnsureWorlds();
        app.OnOpened();
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(2);
    }

    public override void Draw()
    {
        UiInteract.SetWindowHovered(ImGui.IsWindowHovered(
            ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem));

        var scale = UiScale.Current;
        var origin = ImGui.GetCursorScreenPos();
        var available = ImGui.GetContentRegionAvail();
        var frame = new Rect(origin, origin + available);
        var drawList = ImGui.GetWindowDrawList();
        var radius = Metrics.Radius.Card * scale;

        Squircle.Fill(drawList, frame.Min, frame.Max, radius, ImGui.GetColorU32(PlacardTheme.Surface));
        Squircle.Stroke(drawList, frame.Min, frame.Max, radius, ImGui.GetColorU32(PlacardTheme.WindowEdge),
            Metrics.Stroke.Hairline * scale);

        app.Draw(frame.Inset(FrameInset * scale));
        HandleDrag();

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(available);
    }

    public override void OnClose()
    {
        app.OnClosed();
    }

    public void Dispose()
    {
    }

    private void HandleDrag()
    {
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            dragging = false;
        }

        if (!dragging && app.DragHandleHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            dragging = true;
        }

        if (!dragging || !ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            return;
        }

        var delta = ImGui.GetIO().MouseDelta;
        if (delta != Vector2.Zero)
        {
            ImGui.SetWindowPos(ImGui.GetWindowPos() + delta);
        }
    }
}
