using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ShowControl.Models;

public class Cue : INotifyPropertyChanged
{
    private string _name = "";
    private TimeSpan _duration = TimeSpan.Zero;
    private TimeSpan _autoFollowDelay = TimeSpan.Zero;
    private bool _autoFollow = false;
    private CueAction _action = CueAction.Overlay;
    private List<int> _cuesToStop = new();

    public int Number { get; set; }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public TimeSpan Duration
    {
        get => _duration;
        set { _duration = value; OnPropertyChanged(nameof(Duration)); }
    }

    public TimeSpan AutoFollowDelay
    {
        get => _autoFollowDelay;
        set { _autoFollowDelay = value; OnPropertyChanged(nameof(AutoFollowDelay)); }
    }

    public bool AutoFollow
    {
        get => _autoFollow;
        set { _autoFollow = value; OnPropertyChanged(nameof(AutoFollow)); }
    }

    public CueAction Action
    {
        get => _action;
        set { _action = value; OnPropertyChanged(nameof(Action)); }
    }

    public List<int> CuesToStop
    {
        get => _cuesToStop;
        set { _cuesToStop = value; OnPropertyChanged(nameof(CuesToStop)); }
    }

    public string? AudioFilePath { get; set; }
    public string? VideoFilePath { get; set; }
    public List<DmxScene> DmxScenes { get; set; } = new();
    public List<Timeline> Timelines { get; set; } = new();
    public List<MidiCommand> MidiCommands { get; set; } = new();

    // Audio routing settings
    public Dictionary<int, AudioOutput> AudioOutputs { get; set; } = new();

    // Video settings
    public VideoSettings? VideoSettings { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum CueAction
{
    Overlay,    // Let previous cues continue
    Release,    // Stop specified previous cues
    StopAll     // Stop all previous cues
}

public class DmxScene
{
    public string Name { get; set; } = "";
    public int Universe { get; set; }
    public Dictionary<int, byte> ChannelValues { get; set; } = new();
    public TimeSpan FadeTime { get; set; } = TimeSpan.Zero;
}

public class Timeline
{
    public string Name { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public List<TimelineStep> Steps { get; set; } = new();
    public bool Loop { get; set; } = false;
}

public class TimelineStep
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int Universe { get; set; }
    public int Channel { get; set; }
    public byte StartValue { get; set; }
    public byte EndValue { get; set; }
    public EasingType Easing { get; set; } = EasingType.Linear;
}

public enum EasingType
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut,
    Bounce,
    Elastic
}

public class AudioOutput
{
    public int OutputChannel { get; set; }  // 1-8 for different mixer channels
    public float Volume { get; set; } = 1.0f;
    public bool Muted { get; set; } = false;
    public AudioTrackType TrackType { get; set; } = AudioTrackType.Main;
}

public enum AudioTrackType
{
    Main,           // Audience track
    Click,          // Click track for musicians
    Backtrack,      // Backing track
    IEM,            // In-ear monitor mix
    Custom
}

public class DmxUniverse
{
    public int UniverseNumber { get; set; }
    public string DongleType { get; set; } = "";  // ShowXPress, QuickDMX, etc.
    public string ConnectionString { get; set; } = "";  // COM port, IP, etc.
    public byte[] ChannelData { get; set; } = new byte[512];
    public bool IsConnected { get; set; } = false;
}

public class LightFixture
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public int Universe { get; set; }
    public int StartChannel { get; set; }
    public int ChannelCount { get; set; }
    public Dictionary<string, FixtureChannel> Channels { get; set; } = new();
    public FixturePosition Position { get; set; } = new();
}

public class FixtureChannel
{
    public int Offset { get; set; }  // Offset from start channel
    public string Name { get; set; } = "";
    public ChannelType Type { get; set; }
    public byte DefaultValue { get; set; } = 0;
    public byte MinValue { get; set; } = 0;
    public byte MaxValue { get; set; } = 255;
}

public enum ChannelType
{
    Intensity,
    Red,
    Green,
    Blue,
    White,
    Amber,
    UV,
    Pan,
    Tilt,
    PanFine,
    TiltFine,
    Color,
    Gobo,
    Strobe,
    Function,
    Speed
}

public class FixturePosition
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Rotation { get; set; }
}

public class Project
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Modified { get; set; } = DateTime.Now;

    public List<Cue> Cues { get; set; } = new();
    public List<DmxUniverse> Universes { get; set; } = new();
    public List<LightFixture> Fixtures { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();

    public string FilePath { get; set; } = "";
}

public class ShowState
{
    public int CurrentCueNumber { get; set; } = 0;
    public List<int> RunningCues { get; set; } = new();
    public Dictionary<int, TimeSpan> CueStartTimes { get; set; } = new();
    public bool IsPlaying { get; set; } = false;
    public TimeSpan MasterTime { get; set; } = TimeSpan.Zero;
    public List<string> RunningTimelines { get; set; } = new();
    public Dictionary<string, object> ActiveMidiStates { get; set; } = new();
}

public class MidiCommand
{
    public string Name { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public int Channel { get; set; } = 1;
    public MidiMessageType MessageType { get; set; }
    public int Controller { get; set; } = 0;  // For CC messages
    public int Note { get; set; } = 60;       // For Note messages
    public int Value { get; set; } = 127;     // Value or velocity
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;  // Delay from cue start
    public MidiTriggerType TriggerType { get; set; } = MidiTriggerType.OnCueStart;
}

public enum MidiMessageType
{
    NoteOn,
    NoteOff,
    ControlChange,
    ProgramChange,
    PitchBend,
    SystemExclusive,
    Clock,
    Start,
    Stop,
    Continue
}

public enum MidiTriggerType
{
    OnCueStart,
    OnCueEnd,
    OnCueStop,
    Continuous,    // For ongoing CC messages
    OnBeat,        // Sync to beat/BPM
    OnFrame        // Frame-accurate timing
}

public class VideoSettings
{
    public string OutputDisplay { get; set; } = "";  // Display identifier
    public bool Fullscreen { get; set; } = true;
    public VideoPlaybackMode PlaybackMode { get; set; } = VideoPlaybackMode.Normal;
    public bool Loop { get; set; } = false;
    public TimeSpan StartOffset { get; set; } = TimeSpan.Zero;
    public TimeSpan EndOffset { get; set; } = TimeSpan.Zero;
    public float Volume { get; set; } = 1.0f;
    public bool MuteVideo { get; set; } = false;  // Mute video audio, use separate audio track
}

public enum VideoPlaybackMode
{
    Normal,
    Reverse,
    PingPong,
    RandomFrame
}