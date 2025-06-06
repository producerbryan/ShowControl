// SERVICES - All interfaces and implementations in one file for now

using ShowControl.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace ShowControl.Services;

// ===== INTERFACES ONLY =====
public interface ICueSheetService
{
    System.Threading.Tasks.Task<bool> TriggerNextCue();
    System.Threading.Tasks.Task StopAll();
}

public interface IAudioService
{
    System.Threading.Tasks.Task<bool> PlayAudio(string filePath, Dictionary<int, AudioOutput> outputs);
    System.Threading.Tasks.Task<bool> StopAudio(string filePath);
}

public interface IDmxService
{
    System.Threading.Tasks.Task<bool> BlackoutAll();
}

public interface IMidiService
{
    System.Threading.Tasks.Task<bool> SendControlChange(string deviceName, int channel, int controller, int value);
}

public interface IVideoService
{
    System.Threading.Tasks.Task<bool> PlayVideo(string filePath, VideoSettings settings);
}

public interface IStreamDeckService
{
    System.Threading.Tasks.Task<bool> Connect();
    bool IsConnected { get; }
}

public interface IProjectService
{
    System.Threading.Tasks.Task<Project> CreateNewProject(string name);
}

public interface ITimelineService
{
    System.Threading.Tasks.Task<bool> StartTimeline(Timeline timeline, int universe);
}

// ===== STUB IMPLEMENTATIONS =====

// Minimal event arg classes (add these back as we need them)
public class CueEventArgs : EventArgs
{
    public required Cue Cue { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}

public class ShowStateEventArgs : EventArgs
{
    public required ShowState State { get; set; }
}

public class DmxEventArgs : EventArgs
{
    public int Universe { get; set; }
    public string Message { get; set; } = "";
}

public class StreamDeckEventArgs : EventArgs
{
    public int Index { get; set; }
    public float Value { get; set; }
    public bool Pressed { get; set; }
}

public class CueSheetService : ICueSheetService
{
    public async System.Threading.Tasks.Task<bool> TriggerNextCue()
    {
        Console.WriteLine("STUB: Next cue triggered");
        return true;
    }

    public async System.Threading.Tasks.Task StopAll()
    {
        Console.WriteLine("STUB: Stop all");
    }
}

public class AudioService : IAudioService
{
    public async System.Threading.Tasks.Task<bool> PlayAudio(string filePath, Dictionary<int, AudioOutput> outputs)
    {
        Console.WriteLine($"STUB: Playing {filePath}");
        return true;
    }

    public async System.Threading.Tasks.Task<bool> StopAudio(string filePath)
    {
        Console.WriteLine($"STUB: Stopping {filePath}");
        return true;
    }
}

public class DmxService : IDmxService
{
    public async System.Threading.Tasks.Task<bool> BlackoutAll()
    {
        Console.WriteLine("STUB: BLACKOUT");
        return true;
    }
}

public class MidiService : IMidiService
{
    public async System.Threading.Tasks.Task<bool> SendControlChange(string deviceName, int channel, int controller, int value)
    {
        Console.WriteLine($"STUB: MIDI CC {controller}={value}");
        return true;
    }
}

public class VideoService : IVideoService
{
    public async System.Threading.Tasks.Task<bool> PlayVideo(string filePath, VideoSettings settings)
    {
        Console.WriteLine($"STUB: Playing video {filePath}");
        return true;
    }
}

public class StreamDeckService : IStreamDeckService
{
    public bool IsConnected => true;

    public async System.Threading.Tasks.Task<bool> Connect()
    {
        Console.WriteLine("STUB: StreamDeck connected");
        return true;
    }
}

public class ProjectService : IProjectService
{
    public async System.Threading.Tasks.Task<Project> CreateNewProject(string name)
    {
        Console.WriteLine($"STUB: Creating project {name}");
        return new Project { Name = name };
    }
}

public class TimelineService : ITimelineService
{
    public async System.Threading.Tasks.Task<bool> StartTimeline(Timeline timeline, int universe)
    {
        Console.WriteLine($"STUB: Starting timeline {timeline.Name}");
        return true;
    }
}