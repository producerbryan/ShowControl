# ShowControl System - Project Knowledge Base

## 🎯 Project Overview
**Professional Show Control System** - Advanced lighting, audio, video, and MIDI control software for live performances. Designed to replace TheLightingController with superior workflow and frame-accurate timing.

**User Profile**: Professional lighting technician running live shows, needs reliable cue-based control with Page Down advancement, multi-channel audio routing, and precise timing.

## 🏗️ Technical Architecture

### **Framework & Platform**
- **Platform**: Windows desktop application
- **Framework**: C# WPF with .NET 8.0
- **IDE**: Visual Studio 2022
- **Pattern**: MVVM with dependency injection
- **Storage**: JSON project files for universal compatibility

### **Core Dependencies**
```xml
<PackageReference Include="NAudio" Version="2.2.1" />
<PackageReference Include="NAudio.WinMM" Version="2.2.1" />
<PackageReference Include="NAudio.Midi" Version="2.2.1" />
<PackageReference Include="MediaFoundation.NET" Version="1.0.0" />
<PackageReference Include="DirectShowLib" Version="1.0.0" />
<PackageReference Include="FFMpegCore" Version="5.0.2" />
<PackageReference Include="StreamDeckSharp" Version="6.1.0" />
<PackageReference Include="OxyPlot.Wpf" Version="2.1.2" />
<PackageReference Include="Microsoft.Toolkit.Mvvm" Version="7.1.2" />
```

## 🎪 Critical User Requirements

### **Workflow Priorities**
1. **Curve-based programming** (preferred over steps)
2. **Timeline-centric operation** with nested timelines
3. **Program layering** (XY, Color, Intensity, Gobo as separate programs)
4. **Individual fixture control** with quick grouping/ungrouping
5. **Copy/paste everything** for rapid programming
6. **Frame-accurate timing** (60 FPS precision)

### **Hardware Integration**
- **DMX**: Universal dongle support (ShowXPress, QuickDMX, SweetLight, etc.)
- **Audio**: 8-channel output to Behringer X32 mixer
- **MIDI**: In/Out for mixer control and external devices
- **StreamDeck**: XL with encoders/faders for manual control
- **Video**: External display output with click track integration

### **File Formats**
- **Audio**: WAV, MP3, video files with audio tracks
- **Video**: Standard formats with frame-accurate seeking
- **Projects**: JSON with TheLightingController v9 import capability
- **Naming**: Hierarchical (Show-Song-Section-Attribute, YYYYMMDD prefixes)

## 🎵 Audio/Video Workflow

### **Multi-Channel Routing**
```
Output Channels:
├── 1-2: Main audience mix
├── 3-4: (Reserved/future)
├── 5-6: Click track for band/IEM
├── 7-8: Backing tracks for band
```

### **Click Track Integration**
- User converting all audio to video files with visual click
- Frame-accurate synchronization required
- BPM detection and tap tempo functionality
- Visual metronome display

## 🎛️ DMX & Lighting Requirements

### **Fixture Management**
- **16-bit channels**: Smart fine-channel management to prevent jittering
- **Channel pairs**: XY, XxYy with fine-tune lock capability
- **Import**: TheLightingController v9 fixture definitions
- **Universal dongles**: Support multiple DMX interface types

### **Programming Paradigm**
```
Timeline Structure:
├── XY Program (individual per fixture, groupable)
├── Color Program (shared or individual)
├── Intensity Program (master + individual)
├── Gobo Program (synchronized, rarely animated)
└── Special Programs (Zoom, Focus, etc.)
```

### **Grouping Modes**
1. **Synchronized**: All fixtures identical movement
2. **Chase/Offset**: Same pattern with time delays (0-150%+ offset)
3. **Quick switching**: Reassign programs between fixture groups

## 🎯 Cue Sheet System

### **Cue Structure**
```cpp
Cue Properties:
├── Number, Name, Duration
├── Auto-follow with precise timing
├── Action: Overlay/Release/StopAll
├── Media: Audio + Video + DMX + MIDI
├── Timeline groups (nested)
└── Output routing configuration
```

### **Control Methods**
- **Primary**: Page Down (next cue), Page Up (back)
- **Secondary**: Arrow keys + Enter for cue selection
- **Emergency**: Esc (stop all), F1 (blackout)
- **Manual**: StreamDeck faders for live adjustments

### **Timeline Integration**
- Cue sheet entries are timeline groups
- Each cue can contain multiple nested timelines
- Auto-fire vs manual advance per cue
- Real-time conflict detection and resolution

## 🔧 Service Architecture

### **Core Services** (Build Priority Order)
1. **ProjectService**: File I/O, save/load shows
2. **AudioService**: Multi-channel playback, waveform analysis
3. **DmxService**: Universal dongle support, universe management
4. **VideoService**: Frame-accurate playback, external display
5. **MidiService**: Device management, mixer control ✅ IMPLEMENTED
6. **CueSheetService**: Master coordinator ✅ IMPLEMENTED
7. **TimelineService**: Nested timeline execution
8. **StreamDeckService**: Physical controller integration

### **Implementation Status**
- ✅ **CueSheetService**: Complete - handles cue triggering, timing, media coordination
- ✅ **MidiService**: Complete - full MIDI I/O, device management, clock sync
- 🔄 **Project Structure**: Complete - all interfaces and models defined
- ⏳ **Remaining Services**: Need implementation with stub versions first

## 🎨 UI/UX Requirements

### **Layout**
```
Main Window:
├── Left: Cue sheet (350px)
├── Center: Timeline/Waveform editor
├── Right: Manual controls (DMX, Audio, StreamDeck)
└── Bottom: Status bar with show state
```

### **Professional Features**
- **Waveform display**: Visual audio representation
- **Timeline editor**: DaVinci Resolve/FCPX style
- **3D visualization**: Optional fixture positioning
- **Real-time monitoring**: DMX output, MIDI activity, audio levels

## 🚨 Critical Design Principles

### **Reliability**
- Frame-accurate timing (16.67ms precision)
- Graceful error handling for device disconnections
- Emergency stop functionality always available
- Thread-safe operations with proper locking

### **Workflow Efficiency**
- Copy/paste operations for all elements
- Quick fixture reassignment
- Hierarchical naming conventions
- Smart conflict detection and resolution

### **Professional Standards**
- Industry-standard file formats
- Universal hardware compatibility
- Scalable architecture (8+ DMX universes)
- Real-time performance optimization

## 🔄 Current Development Phase

### **Immediate Goals**
1. Get basic project structure building in VS2022
2. Create stub service implementations for all interfaces
3. Implement ProjectService for file operations
4. Build out AudioService with multi-channel support

### **Testing Strategy**
- Start with basic cue triggering (Page Down functionality)
- Test MIDI connectivity with user's mixer
- Verify multi-channel audio routing
- Validate DMX output with ShowXPress dongles

### **Integration Points**
- TheLightingController v9 fixture import
- Ableton Live integration (future)
- External video display management
- StreamDeck XL configuration

## 📝 Development Notes

### **User Feedback Patterns**
- Prefers curve-based over step programming
- Values rapid copy/paste workflow
- Needs frame-accurate timing for professional shows
- Wants nested timeline capability
- Requires reliable emergency controls

### **Technical Challenges**
- 16-bit DMX channel jitter prevention
- Frame-accurate media synchronization
- Universal DMX dongle compatibility
- Real-time conflict resolution in nested timelines
- Multi-channel audio routing with individual volume control

### **Future Enhancements**
- Beat detection and BPM analysis
- Advanced timeline nesting (unlimited depth)
- Ableton Link protocol integration
- Advanced StreamDeck integration
- Cloud-based project collaboration

---

**Last Updated**: 2025-06-06
**Version**: 1.0 (Initial architecture)
**Status**: Foundation phase - building core structure