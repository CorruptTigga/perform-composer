using System.Collections.Generic;

namespace Performer.Midi;

public class MidiPlaylist
{
    public string Name { get; set; } = "Default";
    public List<MidiSong> Songs { get; set; } = new();
    public int CurrentIndex { get; set; } = 0;

    public MidiSong? GetCurrentSong() => Songs.Count > 0 ? Songs[CurrentIndex] : null;

    public bool Next()
    {
        if (CurrentIndex + 1 >= Songs.Count) return false;
        CurrentIndex++;
        return true;
    }
}
