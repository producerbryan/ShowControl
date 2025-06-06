using NAudio.Midi;
using ShowControl.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShowControl.Services;

public class MidiService : IMidiService, IDisposable
{
    private readonly ConcurrentDictionary<string, MidiIn> _inputDevices = new();
    private readonly ConcurrentDictionary<string, MidiOut> _outputDevices = new();
    private readonly ConcurrentDictionary<string, int> _inputDeviceIds = new();
    private readonly ConcurrentDictionary<string, int> _outputDeviceIds = new();

    private readonly System.Timers.Timer _clockTimer = new();
    private readonly object _clockLock = new();
    private bool _clockRunning = false;
    private double _currentBpm = 120.0;

    // MIDI Learn
    private TaskCompletionSource<MidiCommand?>? _midiLearnTask;
    private CancellationTokenSource? _midiLearnCancellation;
    private readonly object _learnLock = new();

    public event EventHandler<MidiEventArgs>? MessageReceived;
    public event EventHandler<MidiEventArgs>? MessageSent;
    public event EventHandler<MidiDeviceEventArgs>? DeviceConnected;
    public event EventHandler<MidiDeviceEventArgs>? DeviceDisconnected;

    public bool IsMidiLearning { get; private set; }

    public MidiService()
    {
        _clockTimer.Elapsed += OnClockTimerElapsed;
        InitializeDeviceLists();
    }

    private void InitializeDeviceLists()
    {
        // Refresh device lists (devices can be hot-plugged)
        RefreshDeviceList();
    }

    private void RefreshDeviceList()
    {
        // This is called periodically or when requested to update available devices
        // NAudio automatically handles device enumeration
    }

    public List<string> GetAvailableInputDevices()
    {
        var devices = new List<string>();
        for (int i = 0; i < MidiIn.NumberOfDevices; i++)
        {
            try
            {
                var caps = MidiIn.DeviceInfo(i);
                devices.Add(caps.ProductName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting MIDI input device {i}: {ex.Message}");
            }
        }
        return devices;
    }

    public List<string> GetAvailableOutputDevices()
    {
        var devices = new List<string>();
        for (int i = 0; i < MidiOut.NumberOfDevices; i++)
        {
            try
            {
                var caps = MidiOut.DeviceInfo(i);
                devices.Add(caps.ProductName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting MIDI output device {i}: {ex.Message}");
            }
        }
        return devices;
    }

    public List<string> GetConnectedInputDevices()
    {
        return _inputDevices.Keys.ToList();
    }

    public List<string> GetConnectedOutputDevices()
    {
        return _outputDevices.Keys.ToList();
    }

    public async Task<bool> ConnectInputDevice(string deviceName)
    {
        try
        {
            if (_inputDevices.ContainsKey(deviceName))
            {
                return true; // Already connected
            }

            // Find device ID by name
            int deviceId = -1;
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var caps = MidiIn.DeviceInfo(i);
                if (caps.ProductName.Equals(deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    deviceId = i;
                    break;
                }
            }

            if (deviceId == -1)
            {
                return false; // Device not found
            }

            var midiIn = new MidiIn(deviceId);
            midiIn.MessageReceived += OnMidiMessageReceived;
            midiIn.ErrorReceived += OnMidiErrorReceived;

            _inputDevices[deviceName] = midiIn;
            _inputDeviceIds[deviceName] = deviceId;

            midiIn.Start();

            DeviceConnected?.Invoke(this, new MidiDeviceEventArgs
            {
                DeviceName = deviceName,
                IsInput = true
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting MIDI input device {deviceName}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ConnectOutputDevice(string deviceName)
    {
        try
        {
            if (_outputDevices.ContainsKey(deviceName))
            {
                return true; // Already connected
            }

            // Find device ID by name
            int deviceId = -1;
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                var caps = MidiOut.DeviceInfo(i);
                if (caps.ProductName.Equals(deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    deviceId = i;
                    break;
                }
            }

            if (deviceId == -1)
            {
                return false; // Device not found
            }

            var midiOut = new MidiOut(deviceId);
            _outputDevices[deviceName] = midiOut;
            _outputDeviceIds[deviceName] = deviceId;

            DeviceConnected?.Invoke(this, new MidiDeviceEventArgs
            {
                DeviceName = deviceName,
                IsInput = false
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting MIDI output device {deviceName}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DisconnectInputDevice(string deviceName)
    {
        try
        {
            if (_inputDevices.TryRemove(deviceName, out var midiIn))
            {
                midiIn.Stop();
                midiIn.MessageReceived -= OnMidiMessageReceived;
                midiIn.ErrorReceived -= OnMidiErrorReceived;
                midiIn.Dispose();

                _inputDeviceIds.TryRemove(deviceName, out _);

                DeviceDisconnected?.Invoke(this, new MidiDeviceEventArgs
                {
                    DeviceName = deviceName,
                    IsInput = true
                });

                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting MIDI input device {deviceName}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DisconnectOutputDevice(string deviceName)
    {
        try
        {
            if (_outputDevices.TryRemove(deviceName, out var midiOut))
            {
                midiOut.Dispose();
                _outputDeviceIds.TryRemove(deviceName, out _);

                DeviceDisconnected?.Invoke(this, new MidiDeviceEventArgs
                {
                    DeviceName = deviceName,
                    IsInput = false
                });

                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting MIDI output device {deviceName}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendMessage(MidiCommand command)
    {
        try
        {
            if (!_outputDevices.TryGetValue(command.DeviceName, out var midiOut))
            {
                return false; // Device not connected
            }

            int midiMessage = CreateMidiMessage(command);
            if (midiMessage != -1)
            {
                midiOut.Send(midiMessage);

                MessageSent?.Invoke(this, new MidiEventArgs
                {
                    DeviceName = command.DeviceName,
                    Command = command,
                    RawData = BitConverter.GetBytes(midiMessage)
                });

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending MIDI message: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendControlChange(string deviceName, int channel, int controller, int value)
    {
        var command = new MidiCommand
        {
            DeviceName = deviceName,
            Channel = channel,
            MessageType = MidiMessageType.ControlChange,
            Controller = controller,
            Value = Math.Clamp(value, 0, 127)
        };

        return await SendMessage(command);
    }

    public async Task<bool> SendNote(string deviceName, int channel, int note, int velocity, bool noteOn = true)
    {
        var command = new MidiCommand
        {
            DeviceName = deviceName,
            Channel = channel,
            MessageType = noteOn ? MidiMessageType.NoteOn : MidiMessageType.NoteOff,
            Note = Math.Clamp(note, 0, 127),
            Value = Math.Clamp(velocity, 0, 127)
        };

        return await SendMessage(command);
    }

    public async Task<bool> SendProgramChange(string deviceName, int channel, int program)
    {
        var command = new MidiCommand
        {
            DeviceName = deviceName,
            Channel = channel,
            MessageType = MidiMessageType.ProgramChange,
            Value = Math.Clamp(program, 0, 127)
        };

        return await SendMessage(command);
    }

    public async Task<bool> SendSysEx(string deviceName, byte[] data)
    {
        try
        {
            if (!_outputDevices.TryGetValue(deviceName, out var midiOut))
            {
                return false;
            }

            midiOut.SendBuffer(data);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending SysEx: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StartMidiClock(double bpm)
    {
        lock (_clockLock)
        {
            _currentBpm = Math.Max(30, Math.Min(300, bpm)); // Reasonable BPM range

            // MIDI clock sends 24 pulses per quarter note
            double intervalMs = (60.0 / _currentBpm / 24.0) * 1000.0;
            _clockTimer.Interval = intervalMs;

            if (!_clockRunning)
            {
                _clockTimer.Start();
                _clockRunning = true;

                // Send MIDI start message to all connected output devices
                _ = SendMidiStart();
            }
        }

        return true;
    }

    public async Task<bool> StopMidiClock()
    {
        lock (_clockLock)
        {
            if (_clockRunning)
            {
                _clockTimer.Stop();
                _clockRunning = false;

                // Send MIDI stop message to all connected output devices
                _ = SendMidiStop();
            }
        }

        return true;
    }

    public async Task<bool> SendMidiStart()
    {
        return await SendRealtimeMessage(0xFA); // MIDI Start
    }

    public async Task<bool> SendMidiStop()
    {
        return await SendRealtimeMessage(0xFC); // MIDI Stop
    }

    public async Task<bool> SendMidiContinue()
    {
        return await SendRealtimeMessage(0xFB); // MIDI Continue
    }

    private async Task<bool> SendRealtimeMessage(byte statusByte)
    {
        try
        {
            foreach (var midiOut in _outputDevices.Values)
            {
                midiOut.Send(statusByte);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending realtime MIDI message: {ex.Message}");
            return false;
        }
    }

    public async Task<MidiCommand?> StartMidiLearn(TimeSpan timeout)
    {
        lock (_learnLock)
        {
            if (IsMidiLearning)
            {
                return null; // Already learning
            }

            IsMidiLearning = true;
            _midiLearnCancellation = new CancellationTokenSource(timeout);
            _midiLearnTask = new TaskCompletionSource<MidiCommand?>();
        }

        try
        {
            return await _midiLearnTask.Task;
        }
        catch (OperationCanceledException)
        {
            return null; // Timeout
        }
        finally
        {
            StopMidiLearnInternal();
        }
    }

    public async Task StopMidiLearn()
    {
        StopMidiLearnInternal();
    }

    private void StopMidiLearnInternal()
    {
        lock (_learnLock)
        {
            if (IsMidiLearning)
            {
                IsMidiLearning = false;
                _midiLearnCancellation?.Cancel();
                _midiLearnCancellation?.Dispose();
                _midiLearnCancellation = null;

                if (_midiLearnTask != null && !_midiLearnTask.Task.IsCompleted)
                {
                    _midiLearnTask.SetResult(null);
                }
                _midiLearnTask = null;
            }
        }
    }

    private void OnMidiMessageReceived(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            var deviceName = GetDeviceNameFromMidiIn(sender as MidiIn);
            var command = ParseMidiMessage(e.MidiEvent, deviceName);

            if (command != null)
            {
                MessageReceived?.Invoke(this, new MidiEventArgs
                {
                    DeviceName = deviceName,
                    Command = command,
                    RawData = BitConverter.GetBytes(e.RawMessage)
                });

                // Handle MIDI learn
                if (IsMidiLearning && _midiLearnTask != null)
                {
                    lock (_learnLock)
                    {
                        if (IsMidiLearning && !_midiLearnTask.Task.IsCompleted)
                        {
                            _midiLearnTask.SetResult(command);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing MIDI message: {ex.Message}");
        }
    }

    private void OnMidiErrorReceived(object? sender, MidiInMessageEventArgs e)
    {
        Console.WriteLine($"MIDI Error: {e.MidiEvent}");
    }

    private void OnClockTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        // Send MIDI clock pulse (0xF8) to all connected output devices
        foreach (var midiOut in _outputDevices.Values)
        {
            try
            {
                midiOut.Send(0xF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending MIDI clock: {ex.Message}");
            }
        }
    }

    private string GetDeviceNameFromMidiIn(MidiIn? midiIn)
    {
        if (midiIn == null) return "Unknown";

        foreach (var kvp in _inputDevices)
        {
            if (kvp.Value == midiIn)
            {
                return kvp.Key;
            }
        }

        return "Unknown";
    }

    private MidiCommand? ParseMidiMessage(MidiEvent midiEvent, string deviceName)
    {
        var command = new MidiCommand { DeviceName = deviceName };

        switch (midiEvent.CommandCode)
        {
            case MidiCommandCode.NoteOn:
                var noteOn = (NoteOnEvent)midiEvent;
                command.MessageType = MidiMessageType.NoteOn;
                command.Channel = noteOn.Channel;
                command.Note = noteOn.NoteNumber;
                command.Value = noteOn.Velocity;
                break;

            case MidiCommandCode.NoteOff:
                var noteOff = (NoteEvent)midiEvent;
                command.MessageType = MidiMessageType.NoteOff;
                command.Channel = noteOff.Channel;
                command.Note = noteOff.NoteNumber;
                command.Value = noteOff.Velocity;
                break;

            case MidiCommandCode.ControlChange:
                var cc = (ControlChangeEvent)midiEvent;
                command.MessageType = MidiMessageType.ControlChange;
                command.Channel = cc.Channel;
                command.Controller = (int)cc.Controller;
                command.Value = cc.ControllerValue;
                break;

            case MidiCommandCode.PatchChange:
                var pc = (PatchChangeEvent)midiEvent;
                command.MessageType = MidiMessageType.ProgramChange;
                command.Channel = pc.Channel;
                command.Value = pc.Patch;
                break;

            case MidiCommandCode.PitchWheelChange:
                var pw = (PitchWheelChangeEvent)midiEvent;
                command.MessageType = MidiMessageType.PitchBend;
                command.Channel = pw.Channel;
                command.Value = pw.Pitch;
                break;

            default:
                return null; // Unsupported message type
        }

        return command;
    }

    private int CreateMidiMessage(MidiCommand command)
    {
        int statusByte = 0;
        int data1 = 0;
        int data2 = 0;

        switch (command.MessageType)
        {
            case MidiMessageType.NoteOn:
                statusByte = 0x90 | (command.Channel - 1);
                data1 = command.Note;
                data2 = command.Value;
                break;

            case MidiMessageType.NoteOff:
                statusByte = 0x80 | (command.Channel - 1);
                data1 = command.Note;
                data2 = command.Value;
                break;

            case MidiMessageType.ControlChange:
                statusByte = 0xB0 | (command.Channel - 1);
                data1 = command.Controller;
                data2 = command.Value;
                break;

            case MidiMessageType.ProgramChange:
                statusByte = 0xC0 | (command.Channel - 1);
                data1 = command.Value;
                // data2 is not used for program change
                break;

            case MidiMessageType.PitchBend:
                statusByte = 0xE0 | (command.Channel - 1);
                // Pitch bend is 14-bit, split across data1 and data2
                data1 = command.Value & 0x7F;
                data2 = (command.Value >> 7) & 0x7F;
                break;

            default:
                return -1; // Unsupported
        }

        return statusByte | (data1 << 8) | (data2 << 16);
    }

    public void Dispose()
    {
        // Stop MIDI clock
        StopMidiClock().Wait();

        // Stop MIDI learn
        StopMidiLearn().Wait();

        // Disconnect all devices
        foreach (var deviceName in _inputDevices.Keys.ToList())
        {
            DisconnectInputDevice(deviceName).Wait();
        }

        foreach (var deviceName in _outputDevices.Keys.ToList())
        {
            DisconnectOutputDevice(deviceName).Wait();
        }

        _clockTimer.Dispose();
        _midiLearnCancellation?.Dispose();
    }
}