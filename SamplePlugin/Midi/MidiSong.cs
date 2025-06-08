using System;
using System.Collections.Generic;

namespace Performer.Midi;

public class MidiSong
{
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<MidiTrackData> Tracks { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public int MicrosecondsPerQuarterNote { get; set; } = 500_000;
    public float SecondsPerBeat => MicrosecondsPerQuarterNote / 1_000_000f;
}
