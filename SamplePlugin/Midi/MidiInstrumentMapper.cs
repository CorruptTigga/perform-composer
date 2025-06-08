using System.Collections.Generic;
using System.Linq;

namespace Performer.Midi;

public static class MidiInstrumentMapper
{
    private static readonly Dictionary<int, FFXIVInstrument> MidiToFfxiv = new()
    {
        // 🎹 Harp/Lutes
        [46] = FFXIVInstrument.Harp,           // Harp
        [0] = FFXIVInstrument.Piano,          // Acoustic Grand Piano
        [24] = FFXIVInstrument.Lute,           // Nylon Guitar (closest match)
        [110] = FFXIVInstrument.Fiddle,        // Fiddle
        [40] = FFXIVInstrument.Violin,         // Violin
        [41] = FFXIVInstrument.Viola,          // Viola
        [42] = FFXIVInstrument.Cello,          // Cello
        [43] = FFXIVInstrument.Double_Bass,    // Contrabass

        // 🎶 Woodwinds
        [73] = FFXIVInstrument.Flute,          // Flute
        [68] = FFXIVInstrument.Oboe,           // Oboe
        [71] = FFXIVInstrument.Clarinet,       // Clarinet
        [78] = FFXIVInstrument.Fife,           // Piccolo (closest match for Fife)
        [75] = FFXIVInstrument.Panpipes,       // Pan Flute

        // 🥁 Percussion (mapped to melodic drum sounds)
        [47] = FFXIVInstrument.Timpani,        // Timpani
        [115] = FFXIVInstrument.Bongo,         // Woodblock (closest match for Bongo)
        [116] = FFXIVInstrument.Bass_Drum,     // Taiko Drum (used as Bass Drum here)
        [117] = FFXIVInstrument.Snare_Drum,    // Melodic Tom (Snare-like)
        [113] = FFXIVInstrument.Cymbal,        // Open Hi-Hat (used for Cymbal)

        // 🎺 Brass
        [56] = FFXIVInstrument.Trumpet,        // Trumpet
        [57] = FFXIVInstrument.Trombone,       // Trombone
        [58] = FFXIVInstrument.Tuba,           // Tuba
        [61] = FFXIVInstrument.Horn,           // French Horn
        [65] = FFXIVInstrument.Saxophone,      // Alto Sax (generic choice)

        // 🎸 Electric Guitar (special case)
        [29] = FFXIVInstrument.Overdriven,     // Overdriven Guitar
        [27] = FFXIVInstrument.Clean,          // Jazz Guitar (Clean)
        [28] = FFXIVInstrument.Muted,          // Muted Guitar
        [30] = FFXIVInstrument.Power_Chords,   // Distortion Guitar
        [31] = FFXIVInstrument.Special         // Guitar Harmonics (closest to “Special”)
    };

    private static readonly Dictionary<FFXIVInstrument, int> FFXIVToMidi =
        MidiToFfxiv.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static FFXIVInstrument? FromMidiProgram(int program)
        => MidiToFfxiv.TryGetValue(program, out var instr) ? instr : null;

    public static int? ToMidiProgram(FFXIVInstrument instrument)
        => FFXIVToMidi.TryGetValue(instrument, out var program) ? program : null;
}
