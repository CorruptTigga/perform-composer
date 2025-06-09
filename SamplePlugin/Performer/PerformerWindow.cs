using System;

using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace DoItBetter.Performer;

public class PerformerWindow : Window, IDisposable
{
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public PerformerWindow(Plugin plugin)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {

    }
}
