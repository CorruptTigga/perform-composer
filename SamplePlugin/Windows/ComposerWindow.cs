using System;
using System.Numerics;

using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace Performer.Windows;

public class ComposerWindow : Window, IDisposable
{
    private Plugin plugin;

    public ComposerWindow(Plugin plugin) : base("Composer##DoItBetter", ImGuiWindowFlags.None)
    {
        this.plugin = plugin;

        Size = new Vector2(400, 200);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        ImGui.Text("Composer");
    }

    public void Dispose() { }
}
