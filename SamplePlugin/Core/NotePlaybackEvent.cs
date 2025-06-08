namespace Performer.Core;

public class NotePlaybackEvent
{
    public char Key { get; set; }               // Mapped key for in-game input
    public double Time { get; set; }            // Playback time in seconds
    public double Duration { get; set; }        // Optional: used if holding key is ever supported
}
