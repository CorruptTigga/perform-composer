using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

using Performer.Midi;

namespace Performer.Core;

public class PlaybackManager : IDisposable
{
    private readonly MidiSong song;
    private readonly Stopwatch stopwatch = new();
    private readonly Timer updateTimer;
    private readonly List<MidiNoteEvent> queuedNotes = new();

    public event Action<MidiNoteEvent>? OnNotePlayed;

    public bool IsPlaying { get; private set; } = false;
    public float PlaybackSpeed { get; set; } = 1.0f;

    public PlaybackManager(MidiSong song)
    {
        this.song = song;

        // Poll roughly 60 times per second
        updateTimer = new Timer(1000.0 / 60.0);
        updateTimer.Elapsed += Update;
        updateTimer.AutoReset = true;
    }

    public void Play()
    {
        if (IsPlaying) return;

        queuedNotes.Clear();
        stopwatch.Restart();

        // Flatten and sort all notes
        queuedNotes.AddRange(song.Tracks.SelectMany(t => t.Notes));
        queuedNotes.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        IsPlaying = true;
        updateTimer.Start();
    }

    public void Stop()
    {
        if (!IsPlaying) return;

        updateTimer.Stop();
        stopwatch.Stop();
        queuedNotes.Clear();
        IsPlaying = false;
    }

    private void Update(object? sender, ElapsedEventArgs e)
    {
        if (!IsPlaying) return;

        double currentTime = stopwatch.Elapsed.TotalSeconds * PlaybackSpeed;

        while (queuedNotes.Count > 0 && queuedNotes[0].StartTime <= currentTime)
        {
            var note = queuedNotes[0];
            queuedNotes.RemoveAt(0);
            OnNotePlayed?.Invoke(note);
        }

        if (currentTime >= song.Duration.TotalSeconds)
            Stop();
    }

    public void Dispose()
    {
        updateTimer.Dispose();
    }
}
