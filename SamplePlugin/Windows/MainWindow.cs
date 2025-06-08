using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Performer.Core;
using Performer.Midi;

namespace Performer.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    public bool IsPlaying = false;
    private PlaybackManager? manager;
    private List<MidiSong> loadedSongs = new();
    private MidiSong? selectedSong;
    private FileDialog midiDialog;
    private float pianoRollZoom = 1.0f;

    public MainWindow(Plugin plugin)
        : base("Performer##DoItBetter", ImGuiWindowFlags.None)
    {
        this.plugin = plugin;
        midiDialog = new FileDialog(
            "Performer Dialog##DoItBetter",
            "Midi Files",
            ".mid,.midi",
            this.plugin.Configuration.DefaultPath,
            "",
            ".mid",
            12,
            true,
            ImGuiFileDialogFlags.SelectOnly
        );
    }

    public void Dispose() { manager?.Dispose(); }

    public override void Draw()
    {
        ImGui.Text("Performer Controls:");

        DrawTopbar();

        DrawSongList();

        DrawMidiTracks();
    }

    private void DrawTopbar()
    {
        if (ImGui.Button("Add MIDI"))
            midiDialog.Show();

        midiDialog.Draw();

        // Load selected songs ONCE
        if (midiDialog.GetIsOk())
        {
            var results = midiDialog.GetResults();

            foreach (var filePath in results)
            {
                if (!File.Exists(filePath))
                {
                    Plugin.Log.Warning($"Invalid file path returned: {filePath}");
                    continue;
                }

                try
                {
                    var loadedSong = MidiLoader.Load(filePath);

                    if (!loadedSongs.Exists(s => s.FilePath == loadedSong.FilePath))
                    {
                        loadedSongs.Add(loadedSong);
                        Logger.Info($"Loaded: {loadedSong.Title} | Tracks: {loadedSong.Tracks.Count}");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Failed to load MIDI: {ex.Message}");
                }
            }

            midiDialog = new FileDialog(
                "Performer Dialog##DoItBetter",
                "Midi Files",
                ".mid,.midi",
                this.plugin.Configuration.DefaultPath,
                "",
                ".mid",
                12,
                true,
                ImGuiFileDialogFlags.SelectOnly
            );
        }

        ImGui.SameLine();

        if (ImGui.Button(IsPlaying ? "Stop Playback" : "Start Playback"))
        {
            if (!IsPlaying)
            {
                if (selectedSong == null)
                {
                    Plugin.Log.Warning("No song loaded.");
                    return;
                }

                manager?.Dispose();
                manager = new PlaybackManager(selectedSong);
                manager.OnNotePlayed += PlayNote;
                manager.Play();
                IsPlaying = true;
            }
            else
            {
                manager?.Stop();
                IsPlaying = false;
            }
        }
    }

    private void DrawSongList()
    {
        // ✅ Draw the song selection area every frame
        ImGui.BeginChild("##songList", new Vector2(0, 200), true);

        for (int i = 0; i < loadedSongs.Count; i++)
        {
            MidiSong song = loadedSongs[i];

            bool isSelected = selectedSong == song;
            bool isPlayingThis = IsPlaying && selectedSong == song;

            if (isPlayingThis)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 1.0f, 0.2f, 1.0f)); // green
            else if (isSelected)
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.85f, 0.2f, 1.0f)); // yellowish

            ImGui.Text($"{(isPlayingThis ? "▶ " : "   ")}{song.Title}");

            if (isPlayingThis || isSelected)
                ImGui.PopStyleColor();

            float fullWidth = ImGui.GetContentRegionAvail().X;
            float buttonWidth = 60f;
            float spacing = ImGui.GetStyle().ItemSpacing.X;

            ImGui.SameLine(fullWidth - (buttonWidth * 2 + spacing));

            if (ImGui.Button($"Select##{i}", new Vector2(buttonWidth, 0)))
            {
                selectedSong = song;
            }

            ImGui.SameLine();

            if (ImGui.Button($"Remove##{i}", new Vector2(buttonWidth, 0)))
            {
                loadedSongs.Remove(song);
            }
        }

        ImGui.EndChild();
    }

    private void DrawMidiTracks()
    {
        ImGui.BeginChild("##midiTracks");

        ImGui.BeginChild("##trackList", new Vector2(200, 0), true);

        if (selectedSong != null)
        {
            foreach (MidiTrackData track in selectedSong.Tracks)
            {
                DrawTrackListing(track);
            }
        }

        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("##pianoRoll", new Vector2(0, 0), true);

        // TODO: Draw piano on the left most side.

        if (selectedSong != null)
        {
            
        }

        ImGui.EndChild();

        ImGui.EndChild();
    }

    private void DrawTrackListing(MidiTrackData track)
    {
        ImGui.BeginChild($"##track-{track.Title}");

        ImGui.Text(track.Title);

        ImGui.EndChild();
    }

    private void PlayNote(MidiNoteEvent note)
    {
        Logger.Info($"[Note] {note.NoteNumber} Velocity {note.Velocity} Start: {note.StartTime}s");
    }
}
