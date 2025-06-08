using System.IO;
using System.Text.Json;

namespace Performer.Core;

public static class PerformerKeyLayoutLoader
{
    public static PerformerKeyLayout LoadFromJson(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PerformerKeyLayout>(json)
               ?? new PerformerKeyLayout(); // fallback empty
    }

    public static void SaveToJson(PerformerKeyLayout layout, string path)
    {
        var json = JsonSerializer.Serialize(layout, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }
}
