using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Performer.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.FirstUseEver;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
    {
        string[] logLevels = Enum.GetNames(typeof(LogLevel));
        int selected = (int)Configuration.LoggingLevel;

        if (ImGui.Combo("Log Level", ref selected, logLevels, logLevels.Length))
        {
            Configuration.LoggingLevel = (LogLevel)selected;
            Configuration.Save();
        }

        if (ImGui.CollapsingHeader("Debug Console"))
        {
            if (ImGui.Button("Clear Log"))
                Logger.Clear();

            ImGui.SameLine();
            if (ImGui.Button("Copy All"))
            {
                var fullLog = string.Join("\n", Logger.LogBuffer);
                ImGui.SetClipboardText(fullLog);
            }

            ImGui.BeginChild("##logscroll", new Vector2(0, 0), true, ImGuiWindowFlags.HorizontalScrollbar);

            foreach (var line in Logger.LogBuffer)
                ImGui.TextUnformatted(line);

            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            ImGui.EndChild();
        }
    }
}
