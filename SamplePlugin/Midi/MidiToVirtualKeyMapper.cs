using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Game.ClientState.Keys;

namespace Performer.Midi;

public static class MidiToVirtualKeyMapper
{
    private static readonly Dictionary<int, VirtualKey> noteToKey = new()
    {
        { 55, VirtualKey.KEY_1 }, { 54, VirtualKey.KEY_2 },{ 53, VirtualKey.KEY_3 }, { 52, VirtualKey.KEY_4 }, { 51, VirtualKey.KEY_5 }, { 50, VirtualKey.KEY_6 }, { 49, VirtualKey.KEY_7 }, { 48, VirtualKey.KEY_8 },
        { 62, VirtualKey.KEY_9 }, { 61, VirtualKey.KEY_0 }, { 60, VirtualKey.Q }, { 59, VirtualKey.W }, { 58, VirtualKey.E }, { 57, VirtualKey.R }, { 56, VirtualKey.T },
        { 69, VirtualKey.Y }, { 68, VirtualKey.U }, { 67, VirtualKey.I }, { 66, VirtualKey.O }, { 65, VirtualKey.P }, { 64, VirtualKey.A }, { 63, VirtualKey.S },

        { 74, VirtualKey.D }, { 73, VirtualKey.F }, { 72, VirtualKey.G }, { 71, VirtualKey.H }, { 70, VirtualKey.J },
        { 79, VirtualKey.K }, { 78, VirtualKey.L }, { 77, VirtualKey.Z }, { 76, VirtualKey.X }, { 75, VirtualKey.C },
        { 84, VirtualKey.V }, { 83, VirtualKey.B }, { 82, VirtualKey.N }, { 81, VirtualKey.M }, { 80, VirtualKey.OEM_COMMA },
    };

    public static bool TryGetKey(int note, out VirtualKey key) => noteToKey.TryGetValue(note, out key);
}
