using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Placard.Core.Game;
using Placard.Core.Housing;
using Placard.Core.Localization;
using Placard.Core.Net;
using Placard.Windows;

namespace Placard;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/placard";
    private const string UserAgent = "Placard/1.0 (+https://github.com/YozoraCho/Placard)";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IAetheryteList AetheryteList { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static INotificationManager Notifications { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private readonly WindowSystem windowSystem = new("Placard");
    private readonly PlacardWindow window;
    private readonly HttpService http;
    private readonly HousingService housing;
    private readonly HousingReminderService reminders;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        var assemblyDirectory = PluginInterface.AssemblyLocation.Directory?.FullName ?? string.Empty;
        Loc.Initialize(Configuration.Language, Path.Combine(assemblyDirectory, "Localization"));
        TimeText.ApplyClockPreference(Configuration.Use24HourClock);

        http = new HttpService(UserAgent);
        var gameData = new GameData(DataManager, ObjectTable);
        var gameMaps = new HousingGameMaps(DataManager, TextureProvider);
        var cacheRoot = new DirectoryInfo(Path.Combine(PluginInterface.ConfigDirectory.FullName, "cache"));
        housing = new HousingService(http, Configuration, gameData, Framework, gameMaps, cacheRoot);
        reminders = new HousingReminderService(Configuration, Framework, Notifications, housing.Watch);

        window = new PlacardWindow(Configuration, housing);
        windowSystem.AddWindow(window);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open Placard, the housing plot browser.",
        });

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleMainUi;
    }

    internal Configuration Configuration { get; }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleMainUi;
        windowSystem.RemoveAllWindows();
        window.Dispose();
        reminders.Dispose();
        housing.Dispose();
        http.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string arguments)
    {
        ToggleMainUi();
    }

    private void ToggleMainUi()
    {
        window.Toggle();
    }
}
