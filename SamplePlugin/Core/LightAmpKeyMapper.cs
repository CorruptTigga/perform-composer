using System.Collections.Generic;

namespace Performer.Core;

public static class LightAmpKeyMapper
{
    // LightAmp / BardMidiPlayer style layout for 84 keys (MIDI 36–119)
    public static Dictionary<int, char> Build()
    {
        const string layout = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
        var keys = new List<char>();

        // Fill keys: shift number row, QWERTY rows, etc. Repeat as needed
        for (char c = '1'; c <= '9'; c++) keys.Add(c);
        keys.Add('0');
        keys.AddRange("QWERTYUIOPASDFGHJKLZXCVBNM");

        var map = new Dictionary<int, char>();
        int startNote = 36; // MIDI note C2
        for (int i = 0; i < keys.Count; i++)
        {
            map[startNote + i] = keys[i];
        }

        return map;
    }
}
