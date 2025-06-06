using ShowControl.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowControl.Services;

public interface ICueSheetService
{
    event EventHandler<CueEventArgs>? CueTriggered;
    event EventHandler<CueEventArgs>? CueCompleted;
    event EventHandler<ShowStateEventArgs>? ShowStateChanged;

    Task LoadProject(string filePath);
    Task SaveProject(string filePath);
    Task<bool> TriggerNextCue();
    Task<bool> TriggerCue(int cueNumber);
    Task<bool> GoBack();
    Task<bool> JumpToCue(int cueNumber);
    Task StopAll();
    Task PauseShow();
    Task ResumeShow();

    List<Cue> GetAllCues();
    Cue? GetCurrentCue();
    ShowState GetShowState();
    Task UpdateCue(Cue cue);
    Task DeleteCue(int cueNumber);
    Task InsertCue(int afterCueNumber, Cue newCue);
}

public interface IAudioService
{
    event EventHandler<AudioEventArgs>? PlaybackStarted;
    event EventHandler<AudioEventArgs>? PlaybackStopped;
    event EventHandler<WaveformAnalysisEventArgs>? WaveformAnalyzed;
    event EventHandler<BeatDetectionEventArgs>? BeatDetected;

    Task<bool> PlayAudio(string filePath, Dictionary<int, AudioOutput> outputs);
    Task<bool> StopAudio(string filePath);
    Task<bool> PauseAudio(string filePath);
    Task<bool> ResumeAudio(string filePath);
    Task<bool> SetVolume(string filePath, int outputChannel, float volume);
    Task<bool> SetMute(string filePath, int outputChannel, bool muted);

    Task<WaveformData> AnalyzeWaveform(string filePath);
    Task<BeatData> DetectBeats(string filePath, int? clickTrackChannel = null);
    Task<bool> SetClickTrackBpm(double bpm);
    Task<bool> TapTempo();

    List<AudioDevice> GetAvailableOutputDevices();
    Task<bool> SetOutputDevice(int outputChannel, int deviceId);
    TimeSpan GetCurrentPosition(string filePath);
    TimeSpan GetDuration(string filePath);
}

public interface IDmxService
{
    event EventHandler<DmxEventArgs>? UniverseConnected;
    event EventHandler<DmxEventArgs>? UniverseDisconnected;
    event EventHandler<DmxDataEventArgs>? DataSent;

    Task<bool> ConnectUniverse(DmxUniverse universe);
    Task<bool> DisconnectUniverse(int universeNumber);
    Task<bool> SendData(int universe, byte[] data);
    Task<bool> SetChannel(int universe, int channel, byte value);
    Task<bool> SetChannels(int universe, Dictionary<int, byte> channels);
    Task<bool> SendScene(DmxScene scene);
    Task<bool> FadeScene(DmxScene scene, TimeSpan fadeTime);

    List<string> GetAvailableDongles();
    List<DmxUniverse> GetConnectedUniverses();
    byte[] GetUniverseData(int universe);
    Task<bool> BlackoutAll();
    Task<bool> SetMasterIntensity(float intensity);
}

public interface IStreamDeckService
{
    event EventHandler<StreamDeckEventArgs>? ButtonPressed;
    event EventHandler<StreamDeckEventArgs>? EncoderChanged;
    event EventHandler<StreamDeckEventArgs>? FaderChanged;

    Task<bool> Connect();
    Task<bool> Disconnect();
    Task<bool> SetButtonImage(int buttonIndex, byte[] imageData);
    Task<bool> SetButtonText(int buttonIndex, string text);
    Task<bool> SetFaderValue(int faderIndex, float value);

    bool IsConnected { get; }
    int ButtonCount { get; }
    int FaderCount { get; }
    int EncoderCount { get; }
}

public interface IProjectService
{
    Task<Project> CreateNewProject(string name);
    Task<Project> LoadProject(string filePath);
    Task<bool> SaveProject(Project project, string filePath);
    Task<bool> SaveProjectAs(Project project, string filePath);
    Task<bool> ExportProject(Project project, string filePath, ExportFormat format);
    Task<Project> ImportProject(string filePath, ImportFormat format);

    List<string> GetRecentProjects();
    Task<bool> ValidateProject(Project project);
}

public interface IMidiService
{
    event EventHandler<MidiEventArgs>? MessageReceived;
    event EventHandler<MidiEventArgs>? MessageSent;
    event EventHandler<MidiDeviceEventArgs>? DeviceConnected;
    event EventHandler<MidiDeviceEventArgs>? DeviceDisconnected;

    Task<bool> ConnectInputDevice(string deviceName);
    Task<bool> ConnectOutputDevice(string deviceName);
    Task<bool> DisconnectInputDevice(string deviceName);
    Task<bool> DisconnectOutputDevice(string deviceName);

    Task<bool> SendMessage(MidiCommand command);
    Task<bool> SendControlChange(string deviceName, int channel, int controller, int value);
    Task<bool> SendNote(string deviceName, int channel, int note, int velocity, bool noteOn = true);
    Task<bool> SendProgramChange(string deviceName, int channel, int program);
    Task<bool> SendSysEx(string deviceName, byte[] data);

    Task<bool> StartMidiClock(double bpm);
    Task<bool> StopMidiClock();
    Task<bool> SendMidiStart();
    Task<bool> SendMidiStop();
    Task<bool> SendMidiContinue();

    List<string> GetAvailableInputDevices();
    List<string> GetAvailableOutputDevices();
    List<string> GetConnectedInputDevices();
    List<string> GetConnectedOutputDevices();

    // MIDI learn functionality
    Task<MidiCommand?> StartMidiLearn(TimeSpan timeout);
    Task StopMidiLearn();
    bool IsMidiLearning { get; }
}

public interface IVideoService
{
    event EventHandler<VideoEventArgs>? PlaybackStarted;
    event EventHandler<VideoEventArgs>? PlaybackStopped;
    event EventHandler<VideoEventArgs>? PlaybackPaused;
    event EventHandler<VideoPositionEventArgs>? PositionChanged;

    Task<bool> PlayVideo(string filePath, VideoSettings settings);
    Task<bool> StopVideo(string filePath);
    Task<bool> PauseVideo(string filePath);
    Task<bool> ResumeVideo(string filePath);
    Task<bool> SeekVideo(string filePath, TimeSpan position);
    Task<bool> SetVideoVolume(string filePath, float volume);

    Task<VideoInfo> GetVideoInfo(string filePath);
    TimeSpan GetCurrentPosition(string filePath);
    TimeSpan GetDuration(string filePath);
    List<string> GetAvailableDisplays();
    Task<bool> SetOutputDisplay(string filePath, string displayName);

    // Frame-accurate seeking for click track sync
    Task<bool> SeekToFrame(string filePath, long frameNumber);
    long GetCurrentFrame(string filePath);
    long GetTotalFrames(string filePath);
}

public interface ITimelineService
{
    event EventHandler<TimelineEventArgs>? TimelineStarted;
    event EventHandler<TimelineEventArgs>? TimelineCompleted;
    event EventHandler<TimelineStepEventArgs>? StepExecuted;

    Task<bool> StartTimeline(Timeline timeline, int universe);
    Task<bool> StopTimeline(string timelineName);
    Task<bool> PauseTimeline(string timelineName);
    Task<bool> ResumeTimeline(string timelineName);

    Task<Timeline> CreateTimeline(string name, TimeSpan duration);
    Task<bool> AddStep(string timelineName, TimelineStep step);
    Task<bool> RemoveStep(string timelineName, int stepIndex);
    Task<bool> CopyTimelineToFixture(string sourceTimelineName, int sourceFixture, int targetFixture, TimeSpan offset);

    List<Timeline> GetRunningTimelines();
    TimeSpan GetTimelinePosition(string timelineName);
}

// Event argument classes
public class CueEventArgs : EventArgs
{
    public Cue Cue { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}

public class ShowStateEventArgs : EventArgs
{
    public ShowState State { get; set; }
}

public class AudioEventArgs : EventArgs
{
    public string FilePath { get; set; } = "";
    public Dictionary<int, AudioOutput> Outputs { get; set; } = new();
}

public class VideoEventArgs : EventArgs
{
    public string FilePath { get; set; } = "";
    public VideoSettings Settings { get; set; } = new();
}

public class VideoPositionEventArgs : EventArgs
{
    public string FilePath { get; set; } = "";
    public TimeSpan Position { get; set; }
    public long FrameNumber { get; set; }
}

public class MidiEventArgs : EventArgs
{
    public string DeviceName { get; set; } = "";
    public MidiCommand Command { get; set; } = new();
    public byte[] RawData { get; set; } = Array.Empty<byte>();
}

public class MidiDeviceEventArgs : EventArgs
{
    public string DeviceName { get; set; } = "";
    public bool IsInput { get; set; }
}

public class WaveformAnalysisEventArgs : EventArgs
{
    public WaveformData Data { get; set; }
}

public class BeatDetectionEventArgs : EventArgs
{
    public BeatData Data { get; set; }
    public TimeSpan Position { get; set; }
}

public class DmxEventArgs : EventArgs
{
    public int Universe { get; set; }
    public string Message { get; set; } = "";
}

public class DmxDataEventArgs : EventArgs
{
    public int Universe { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public class StreamDeckEventArgs : EventArgs
{
    public int Index { get; set; }
    public float Value { get; set; }
    public bool Pressed { get; set; }
}

public class TimelineEventArgs : EventArgs
{
    public Timeline Timeline { get; set; }
    public int Universe { get; set; }
}

public class TimelineStepEventArgs : EventArgs
{
    public TimelineStep Step { get; set; }
    public Timeline Timeline { get; set; }
}

// Supporting data classes
public class WaveformData
{
    public float[] Samples { get; set; } = Array.Empty<float>();
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public TimeSpan Duration { get; set; }
}

public class BeatData
{
    public List<TimeSpan> BeatTimes { get; set; } = new();
    public double AverageBpm { get; set; }
    public double ConfidenceLevel { get; set; }
}

public class VideoInfo
{
    public TimeSpan Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public long TotalFrames { get; set; }
    public string Codec { get; set; } = "";
    public bool HasAudio { get; set; }
}

public class AudioDevice
{
    public int DeviceId { get; set; }
    public string Name { get; set; } = "";
    public int MaxChannels { get; set; }
    public bool IsDefault { get; set; }
}

public enum ExportFormat
{
    Json,
    Xml,
    Csv
}

public enum ImportFormat
{
    Json,
    Xml,
    ShowXpress,
    TheLightingController
}