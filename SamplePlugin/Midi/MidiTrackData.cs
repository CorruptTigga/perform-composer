using System.Collections.Generic;

namespace Performer.Midi;

public class MidiTrackData
{
    public string Title { get; set; } = "Untitled";
    public List<MidiNoteEvent> Notes { get; set; } = new();
    public int Channel { get; set; } = 0;
}
