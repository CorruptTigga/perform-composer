using System;

using Dalamud.Configuration;

namespace Performer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public LogLevel LoggingLevel { get; set; } = LogLevel.Info;
    public string DefaultPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

public enum LogLevel
{
    Off,
    Info,
    Debug
}
