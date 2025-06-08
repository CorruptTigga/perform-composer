using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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
    private float pianoRollZoomX = 1.0f;
    private Vector2 pianoRollScroll = Vector2.Zero;
    private const int FirstNote = 48; // C3
    private const int LastNote = 84; // B5 (C6 is 84, exclusive)
    private const float KeyHeight = 18f;

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
        float childHeight = ImGui.GetContentRegionAvail().Y;

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

        // Piano
        ImGui.BeginChild("##trackPiano", new Vector2(40, 0), false);
        DrawPianoKeys(childHeight);
        ImGui.EndChild();

        ImGui.SameLine();

        // Notes
        ImGui.BeginChild("##trackRoll", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar);
        HandlePianoRollZoom();
        DrawPianoRoll(ImGui.GetContentRegionAvail().X, childHeight);
        ImGui.EndChild();

        ImGui.EndChild();

        ImGui.EndChild();
    }

    private void DrawTrackListing(MidiTrackData track)
    {
        ImGui.BeginChild($"##track-{track.Name}", new Vector2(), true);

        Vector4 color = GetColorForTrack(track.Name);
        ImGui.ColorButton($"##color-{track.Name}", color, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(10, 10));
        ImGui.SameLine();
        ImGui.Text(track.Name);

        ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), $"{MidiInstrumentMapper.FromMidiProgram(track.Channel)}");

        ImGui.EndChild();
    }

    private static readonly string[] NoteNames = {
        "C", "C#", "D", "D#", "E", "F",
        "F#", "G", "G#", "A", "A#", "B"
    };

    private string GetNoteName(int midiNote)
    {
        int octave = (midiNote / 12) - 1;
        int noteIndex = midiNote % 12;
        return $"{NoteNames[noteIndex]}{octave}";
    }

    private void DrawPianoKeys(float height)
    {
        var whiteKeys = new HashSet<int> { 0, 2, 4, 5, 7, 9, 11 }; // White keys

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 basePos = ImGui.GetCursorScreenPos();

        int noteCount = LastNote - FirstNote + 1;

        for (int i = 0; i < noteCount; i++)
        {
            int note = LastNote - i; // Draw top-down
            float y = i * KeyHeight - pianoRollScroll.Y;
            Vector2 topLeft = new(basePos.X, basePos.Y + y);
            Vector2 bottomRight = new(basePos.X + 40, basePos.Y + y + KeyHeight);

            if (bottomRight.Y < basePos.Y || topLeft.Y > basePos.Y + height)
                continue;

            bool isWhite = whiteKeys.Contains(note % 12);
            var color = isWhite ? new Vector4(0.9f, 0.9f, 0.9f, 1f) : new Vector4(0.2f, 0.2f, 0.2f, 1f);
            drawList.AddRectFilled(topLeft, bottomRight, ImGui.GetColorU32(color));
            drawList.AddRect(topLeft, bottomRight, ImGui.GetColorU32(new Vector4(0, 0, 0, 1)));

            // Draw note label
            string noteName = GetNoteName(note);
            Vector2 textSize = ImGui.CalcTextSize(noteName);
            Vector2 textPos = new Vector2(
                topLeft.X + (40 - textSize.X) * 0.5f,
                topLeft.Y + (KeyHeight - textSize.Y) * 0.5f
            );

            var textColor = isWhite
                ? ImGui.GetColorU32(new Vector4(0, 0, 0, 1))   // Black text on white key
                : ImGui.GetColorU32(new Vector4(1, 1, 1, 1));  // White text on black key

            drawList.AddText(textPos, textColor, noteName);
        }

        ImGui.Dummy(new Vector2(40, noteCount * KeyHeight));
    }

    private void HandlePianoRollZoom()
    {
        if (ImGui.IsWindowHovered() && ImGui.GetIO().KeyCtrl)
        {
            float wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0)
            {
                float prevZoom = pianoRollZoomX;
                float newZoom = Math.Clamp(prevZoom + wheel * 0.1f, 0.2f, 5.0f);

                if (Math.Abs(prevZoom - newZoom) > 0.001f)
                {
                    float zoomFactor = newZoom / prevZoom;
                    pianoRollZoomX = newZoom;

                    // Anchor zoom to the left
                    pianoRollScroll.X *= zoomFactor;

                    // Clamp scroll
                    float songDuration = (float)(selectedSong?.Duration.TotalSeconds ?? 0.0);
                    float totalWidth = songDuration * 100f * pianoRollZoomX;
                    float visibleWidth = ImGui.GetContentRegionAvail().X;

                    pianoRollScroll.X = Math.Clamp(pianoRollScroll.X, 0, Math.Max(0, totalWidth - visibleWidth));
                }
            }
        }
    }

    private void DrawPianoRoll(float width, float height)
    {
        if (selectedSong == null) return;

        const float timeToPixels = 100f;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 basePos = ImGui.GetCursorScreenPos();

        float songDuration = (float)selectedSong.Duration.TotalSeconds + 1.0f;
        float totalContentWidth = songDuration * timeToPixels * pianoRollZoomX;

        DrawGridLines(basePos, totalContentWidth, height);

        foreach (var track in selectedSong.Tracks)
        {
            Vector4 trackColor = GetColorForTrack(track.Name);
            uint color = ImGui.GetColorU32(trackColor);

            foreach (var note in track.Notes)
            {
                if (note.NoteNumber < FirstNote || note.NoteNumber > LastNote)
                    continue;

                float startX = note.StartTime * timeToPixels * pianoRollZoomX - pianoRollScroll.X;
                float endX = startX + note.Duration * timeToPixels * pianoRollZoomX;

                float noteY = (LastNote - note.NoteNumber) * KeyHeight - pianoRollScroll.Y;
                Vector2 topLeft = new(basePos.X + startX, basePos.Y + noteY);
                Vector2 bottomRight = new(basePos.X + endX, basePos.Y + noteY + KeyHeight);

                if (bottomRight.Y < basePos.Y || topLeft.Y > basePos.Y + height)
                    continue;

                drawList.AddRectFilled(topLeft, bottomRight, color, 2.0f);
                drawList.AddRect(topLeft, bottomRight, ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), 2.0f);
            }
        }

        float visibleWidth = ImGui.GetContentRegionAvail().X;
        float totalWidth = songDuration * timeToPixels * pianoRollZoomX;
        pianoRollScroll.X = Math.Clamp(pianoRollScroll.X, 0, Math.Max(0, totalContentWidth - visibleWidth));
        ImGui.Dummy(new Vector2(totalWidth, (LastNote - FirstNote + 1) * KeyHeight));
    }

    private void DrawGridLines(Vector2 basePos, float totalContentWidth, float height)
    {
        if (selectedSong == null)
            return;

        float secondsPerBeat = selectedSong.SecondsPerBeat;

        // Clamp invalid or zero values
        if (secondsPerBeat <= 0 || float.IsNaN(secondsPerBeat) || float.IsInfinity(secondsPerBeat))
            secondsPerBeat = 0.5f; // fallback to 120 BPM

        int beatsPerBar = 4;
        float timeToPixels = 100f;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        uint subBeatColor = ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 0.3f));
        uint majorBarColor = ImGui.GetColorU32(new Vector4(1.0f, 0.2f, 0.2f, 0.6f));

        float beatSpacing = secondsPerBeat * timeToPixels * pianoRollZoomX;
        if (beatSpacing < 1f) return; // prevent too-dense grid lines

        int beatIndex = 0;

        for (float x = 0; x < totalContentWidth; x += beatSpacing, beatIndex++)
        {
            float drawX = basePos.X + x - pianoRollScroll.X;

            bool isBar = (beatIndex % beatsPerBar) == 0;
            uint color = isBar ? majorBarColor : subBeatColor;
            float thickness = isBar ? 2f : 1f;

            Vector2 top = new(drawX, basePos.Y);
            Vector2 bottom = new(drawX, basePos.Y + height);

            drawList.AddLine(top, bottom, color, thickness);

            if (isBar)
            {
                drawList.AddText(new Vector2(drawX + 3, basePos.Y), ImGui.GetColorU32(Vector4.One), $"Bar {beatIndex / beatsPerBar + 1}");
            }
        }
    }

    private static Vector4 GetColorForTrack(string trackName)
    {
        // Hash to consistent hue
        int hash = trackName.GetHashCode();
        float hue = (hash & 0xFFFFFF) / (float)0xFFFFFF;

        // HSV to RGB
        Vector3 rgb = HsvToRgb(hue, 0.6f, 0.9f);
        return new Vector4(rgb, 1.0f);
    }

    private static Vector3 HsvToRgb(float h, float s, float v)
    {
        int i = (int)(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);
        i = i % 6;

        return i switch
        {
            0 => new Vector3(v, t, p),
            1 => new Vector3(q, v, p),
            2 => new Vector3(p, v, t),
            3 => new Vector3(p, q, v),
            4 => new Vector3(t, p, v),
            5 => new Vector3(v, p, q),
            _ => new Vector3(1, 1, 1),
        };
    }


    private void PlayNote(MidiNoteEvent note)
    {
        Logger.Info($"[Note] {note.NoteNumber} Start: {note.StartTime}s");
    }
}
