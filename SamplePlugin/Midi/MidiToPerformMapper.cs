using System.Collections.Generic;

using Performer.Core;

namespace Performer.Midi;

public class MidiToPerformMapper
{
    private readonly List<NotePlaybackEvent> _scheduledNotes = new();

    public MidiToPerformMapper(
        MidiTrackData track,
        PerformerKeyLayout keyLayout,
        double startDelaySeconds = 0.0)
    {
        foreach (var note in track.Notes)
        {
            if (!keyLayout.TryGetKeyForNote(note.NoteNumber, out var key))
                continue;

            _scheduledNotes.Add(new NotePlaybackEvent
            {
                Key = key,
                Time = note.StartTime + startDelaySeconds,
                Duration = note.Duration
            });
        }

        _scheduledNotes.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

    public IReadOnlyList<NotePlaybackEvent> Scheduled => _scheduledNotes;
}
