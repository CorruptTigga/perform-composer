using System;
using System.Collections.Generic;

using Dalamud.Plugin.Services;

namespace Performer;

public static class Logger
{
    private static IPluginLog? pluginLog;
    private static Configuration? config;

    private const int MaxLines = 100;
    public static readonly Queue<string> LogBuffer = new();

    public static void Init(IPluginLog log, Configuration configuration)
    {
        pluginLog = log;
        config = configuration;
    }

    public static void Info(string message)
    {
        if (config?.LoggingLevel < LogLevel.Info)
            return;

        Write("[Info]", message);
    }

    public static void Debug(string message)
    {
        if (config?.LoggingLevel < LogLevel.Debug)
            return;

        Write("[Debug]", message);
    }

    private static void Write(string prefix, string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss} {prefix} {message}";

        if (LogBuffer.Count >= MaxLines)
            LogBuffer.Dequeue();

        LogBuffer.Enqueue(line);

        pluginLog?.Information(message); // Send to Dalamud log
    }

    public static void Clear() => LogBuffer.Clear();
}
