using System.Threading.Tasks;

using Dalamud.Game.ClientState.Keys;

using Performer.Midi;

namespace Performer.Core;

public class InputHandler
{
    private readonly Plugin plugin;
    private readonly MidiSong song;

    public InputHandler(Plugin plugin, MidiSong song)
    {
        this.plugin = plugin;
        this.song = song;
    }

    public void HandleNote(MidiNoteEvent note)
    {
        if (!MidiToVirtualKeyMapper.TryGetKey(note.NoteNumber, out var key))
            return;

        _ = PlayNoteAsync(key, note.Duration);
    }

    private async Task PlayNoteAsync(VirtualKey key, float durationTicks)
    {
        plugin.SendKey(key, 1); // Press

        double durationMs = (durationTicks / song.TicksPerQuarterNote) * (60000.0 / song.BPM);

        if (durationMs < 100)
        {
            plugin.SendKey(key, 0); // Tap immediately
        }
        else
        {
            await Task.Delay((int)durationMs);
            plugin.SendKey(key, 0); // Release
        }
    }
}
