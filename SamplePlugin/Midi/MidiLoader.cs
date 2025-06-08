using System;
using System.IO;
using System.Linq;

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Performer.Midi;

public static class MidiLoader
{
    public static MidiSong Load(string path)
    {
        var midiFile = MidiFile.Read(path);
        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();

        var tracks = notes
            .GroupBy(n => n.Channel)
            .Select(group => new MidiTrackData
            {
                Channel = group.Key,
                Notes = group.Select(n =>
                {
                    var metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(n.Time, tempoMap);
                    var startSeconds = metricTime.TotalMicroseconds / 1_000_000.0;

                    var durationTime = LengthConverter.ConvertTo<MetricTimeSpan>(n.Length, n.Time, tempoMap);
                    var durationSeconds = durationTime.TotalMicroseconds / 1_000_000.0;

                    return new MidiNoteEvent
                    {
                        NoteNumber = n.NoteNumber,
                        Velocity = n.Velocity,
                        StartTime = (float)startSeconds,
                        Duration = (float)durationSeconds
                    };
                }).ToList()
            }).ToList();

        var durationMetric = midiFile.GetDuration<MetricTimeSpan>();
        var totalDurationSeconds = durationMetric.TotalMicroseconds / 1_000_000.0;

        return new MidiSong
        {
            FilePath = path,
            Title = Path.GetFileNameWithoutExtension(path),
            Tracks = tracks,
            Duration = TimeSpan.FromSeconds(totalDurationSeconds)
        };
    }
}
