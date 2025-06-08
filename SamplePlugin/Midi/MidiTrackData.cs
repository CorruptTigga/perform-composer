using System.Collections.Generic;

namespace Performer.Midi;

public class MidiTrackData
{
    public string Name { get; set; } = "Untitled";
    public int Channel { get; set; } = 0;
    public List<MidiNoteEvent> Notes { get; set; } = new();
}
