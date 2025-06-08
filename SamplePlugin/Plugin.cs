using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Performer.Windows;

namespace Performer;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandPerformer = "/performer";
    private const string CommandComposer = "/composer";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Performer");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow PerformerWindow { get; init; }
    private ComposerWindow ComposerWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Logger.Init(Log, Configuration);

        ConfigWindow = new ConfigWindow(this);
        PerformerWindow = new MainWindow(this);
        ComposerWindow = new ComposerWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(PerformerWindow);
        WindowSystem.AddWindow(ComposerWindow);

        CommandManager.AddHandler(CommandPerformer, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Performer window."
        });

        CommandManager.AddHandler(CommandComposer, new CommandInfo(OnComposerCommand)
        {
            HelpMessage = "Opens the Composer window."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        PerformerWindow.Dispose();
        ComposerWindow.Dispose();

        CommandManager.RemoveHandler(CommandPerformer);
        CommandManager.RemoveHandler(CommandComposer);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void OnComposerCommand(string command, string args)
    {
        ToggleComposerUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => PerformerWindow.Toggle();
    public void ToggleComposerUI() => ComposerWindow.Toggle();
}
