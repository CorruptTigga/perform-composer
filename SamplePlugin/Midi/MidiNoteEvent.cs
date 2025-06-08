namespace Performer.Midi;

public class MidiNoteEvent
{
    public int NoteNumber { get; set; }
    public float StartTime { get; set; }
    public float Duration { get; set; }
    public int Velocity { get; set; }

    public Melanchall.DryWetMidi.Interaction.Note? SourceNote { get; set; }
}
