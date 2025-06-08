using System.Collections.Generic;

namespace Performer.Core;

public class PerformerKeyLayout
{
    public Dictionary<int, char> NoteToKeyMap { get; init; } = new();

    public bool TryGetKeyForNote(int midiNote, out char key) =>
        NoteToKeyMap.TryGetValue(midiNote, out key);
}
