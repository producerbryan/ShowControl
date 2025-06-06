using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ShowControl.Models;
using ShowControl.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ShowControl.ViewModels;

public class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly ICueSheetService _cueSheetService;
    private readonly IAudioService _audioService;
    private readonly IDmxService _dmxService;
    private readonly IStreamDeckService _streamDeckService;
    private readonly IProjectService _projectService;

    private Project? _currentProject;
    private Cue? _selectedCue;
    private ShowState? _showState;
    private bool _isShowRunning = false;
    private string _statusText = "Ready";

    public MainWindowViewModel(
        ICueSheetService cueSheetService,
        IAudioService audioService,
        IDmxService dmxService,
        IStreamDeckService streamDeckService,
        IProjectService projectService)
    {
        _cueSheetService = cueSheetService;
        _audioService = audioService;
        _dmxService = dmxService;
        _streamDeckService = streamDeckService;
        _projectService = projectService;

        InitializeCommands();
        InitializeEventHandlers();
        InitializeServices();
    }

    #region Properties

    public ObservableCollection<Cue> Cues { get; } = new();

    public Cue? SelectedCue
    {
        get => _selectedCue;
        set => SetProperty(ref _selectedCue, value);
    }

    public bool IsShowRunning
    {
        get => _isShowRunning;
        set => SetProperty(ref _isShowRunning, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public Project? CurrentProject
    {
        get => _currentProject;
        set => SetProperty(ref _currentProject, value);
    }

    #endregion

    #region Commands

    public ICommand? NextCueCommand { get; private set; }
    public ICommand? PreviousCueCommand { get; private set; }
    public ICommand? PlayPauseCommand { get; private set; }
    public ICommand? StopAllCommand { get; private set; }
    public ICommand? BlackoutCommand { get; private set; }
    public ICommand? TriggerSelectedCueCommand { get; private set; }

    // File Commands
    public ICommand? NewProjectCommand { get; private set; }
    public ICommand? OpenProjectCommand { get; private set; }
    public ICommand? SaveProjectCommand { get; private set; }
    public ICommand? SaveAsProjectCommand { get; private set; }
    public ICommand? ExitCommand { get; private set; }

    // Edit Commands
    public ICommand? UndoCommand { get; private set; }
    public ICommand? RedoCommand { get; private set; }
    public ICommand? PreferencesCommand { get; private set; }

    // Show Commands  
    public ICommand? ResetShowCommand { get; private set; }

    // Tools Commands
    public ICommand? DmxMonitorCommand { get; private set; }
    public ICommand? AudioAnalyzerCommand { get; private set; }
    public ICommand? FixtureLibraryCommand { get; private set; }
    public ICommand? StreamDeckSetupCommand { get; private set; }

    // Help Commands
    public ICommand? UserManualCommand { get; private set; }
    public ICommand? AboutCommand { get; private set; }

    #endregion

    private void InitializeCommands()
    {
        // Show Control Commands
        NextCueCommand = new AsyncRelayCommand(NextCue);
        PreviousCueCommand = new AsyncRelayCommand(PreviousCue);
        PlayPauseCommand = new AsyncRelayCommand(PlayPause);
        StopAllCommand = new AsyncRelayCommand(StopAll);
        BlackoutCommand = new AsyncRelayCommand(Blackout);
        TriggerSelectedCueCommand = new AsyncRelayCommand(TriggerSelectedCue);

        // File Commands
        NewProjectCommand = new AsyncRelayCommand(NewProject);
        OpenProjectCommand = new AsyncRelayCommand(OpenProject);
        SaveProjectCommand = new AsyncRelayCommand(SaveProject);
        SaveAsProjectCommand = new AsyncRelayCommand(SaveAsProject);
        ExitCommand = new RelayCommand(Exit);

        // Edit Commands
        UndoCommand = new RelayCommand(Undo, () => false); // TODO: Implement undo/redo
        RedoCommand = new RelayCommand(Redo, () => false);
        PreferencesCommand = new RelayCommand(ShowPreferences);

        // Show Commands
        ResetShowCommand = new AsyncRelayCommand(ResetShow);

        // Tools Commands
        DmxMonitorCommand = new RelayCommand(ShowDmxMonitor);
        AudioAnalyzerCommand = new RelayCommand(ShowAudioAnalyzer);
        FixtureLibraryCommand = new RelayCommand(ShowFixtureLibrary);
        StreamDeckSetupCommand = new RelayCommand(ShowStreamDeckSetup);

        // Help Commands
        UserManualCommand = new RelayCommand(ShowUserManual);
        AboutCommand = new RelayCommand(ShowAbout);
    }

    private void InitializeEventHandlers()
    {
        _cueSheetService.CueTriggered += OnCueTriggered;
        _cueSheetService.CueCompleted += OnCueCompleted;
        _cueSheetService.ShowStateChanged += OnShowStateChanged;

        _dmxService.UniverseConnected += OnUniverseConnected;
        _dmxService.UniverseDisconnected += OnUniverseDisconnected;

        _streamDeckService.ButtonPressed += OnStreamDeckButtonPressed;
        _streamDeckService.FaderChanged += OnStreamDeckFaderChanged;
        _streamDeckService.EncoderChanged += OnStreamDeckEncoderChanged;
    }

    private async void InitializeServices()
    {
        try
        {
            StatusText = "Initializing services...";

            // Connect to StreamDeck if available
            await _streamDeckService.Connect();

            // Initialize DMX universes (will be configured in preferences)
            // await InitializeDmxUniverses();

            StatusText = "Ready";
        }
        catch (Exception ex)
        {
            StatusText = $"Initialization error: {ex.Message}";
        }
    }

    #region Command Implementations

    private async Task NextCue()
    {
        try
        {
            await _cueSheetService.TriggerNextCue();
        }
        catch (Exception ex)
        {
            StatusText = $"Error triggering next cue: {ex.Message}";
        }
    }

    private async Task PreviousCue()
    {
        try
        {
            await _cueSheetService.GoBack();
        }
        catch (Exception ex)
        {
            StatusText = $"Error going to previous cue: {ex.Message}";
        }
    }

    private async Task PlayPause()
    {
        try
        {
            if (IsShowRunning)
            {
                await _cueSheetService.PauseShow();
            }
            else
            {
                await _cueSheetService.ResumeShow();
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error with play/pause: {ex.Message}";
        }
    }

    private async Task StopAll()
    {
        try
        {
            await _cueSheetService.StopAll();
        }
        catch (Exception ex)
        {
            StatusText = $"Error stopping all: {ex.Message}";
        }
    }

    private async Task Blackout()
    {
        try
        {
            await _dmxService.BlackoutAll();
            StatusText = "Blackout activated";
        }
        catch (Exception ex)
        {
            StatusText = $"Error with blackout: {ex.Message}";
        }
    }

    private async Task TriggerSelectedCue()
    {
        if (SelectedCue != null)
        {
            try
            {
                await _cueSheetService.TriggerCue(SelectedCue.Number);
            }
            catch (Exception ex)
            {
                StatusText = $"Error triggering cue {SelectedCue.Number}: {ex.Message}";
            }
        }
    }

    private async Task NewProject()
    {
        try
        {
            var project = await _projectService.CreateNewProject("New Show");
            CurrentProject = project;
            Cues.Clear();
            StatusText = "New project created";
        }
        catch (Exception ex)
        {
            StatusText = $"Error creating new project: {ex.Message}";
        }
    }

    private async Task OpenProject()
    {
        try
        {
            // TODO: Show file dialog
            StatusText = "Open project not implemented yet";
        }
        catch (Exception ex)
        {
            StatusText = $"Error opening project: {ex.Message}";
        }
    }

    private async Task SaveProject()
    {
        if (CurrentProject?.FilePath != null)
        {
            try
            {
                await _projectService.SaveProject(CurrentProject, CurrentProject.FilePath);
                StatusText = "Project saved";
            }
            catch (Exception ex)
            {
                StatusText = $"Error saving project: {ex.Message}";
            }
        }
        else
        {
            await SaveAsProject();
        }
    }

    private async Task SaveAsProject()
    {
        try
        {
            // TODO: Show file dialog
            StatusText = "Save As not implemented yet";
        }
        catch (Exception ex)
        {
            StatusText = $"Error saving project: {ex.Message}";
        }
    }

    private async Task ResetShow()
    {
        try
        {
            await _cueSheetService.StopAll();
            // Reset to beginning
            StatusText = "Show reset";
        }
        catch (Exception ex)
        {
            StatusText = $"Error resetting show: {ex.Message}";
        }
    }

    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void Undo() { /* TODO: Implement */ }
    private void Redo() { /* TODO: Implement */ }
    private void ShowPreferences() { /* TODO: Implement */ }
    private void ShowDmxMonitor() { /* TODO: Implement */ }
    private void ShowAudioAnalyzer() { /* TODO: Implement */ }
    private void ShowFixtureLibrary() { /* TODO: Implement */ }
    private void ShowStreamDeckSetup() { /* TODO: Implement */ }
    private void ShowUserManual() { /* TODO: Implement */ }
    private void ShowAbout() { /* TODO: Implement */ }

    #endregion

    #region Event Handlers

    private void OnCueTriggered(object? sender, CueEventArgs e)
    {
        StatusText = $"Cue {e.Cue.Number}: {e.Cue.Name} started";
        SelectedCue = e.Cue;
    }

    private void OnCueCompleted(object? sender, CueEventArgs e)
    {
        StatusText = $"Cue {e.Cue.Number}: {e.Cue.Name} completed";
    }

    private void OnShowStateChanged(object? sender, ShowStateEventArgs e)
    {
        _showState = e.State;
        IsShowRunning = e.State.IsPlaying;
    }

    private void OnUniverseConnected(object? sender, DmxEventArgs e)
    {
        StatusText = $"DMX Universe {e.Universe} connected";
    }

    private void OnUniverseDisconnected(object? sender, DmxEventArgs e)
    {
        StatusText = $"DMX Universe {e.Universe} disconnected";
    }

    private void OnStreamDeckButtonPressed(object? sender, StreamDeckEventArgs e)
    {
        // Handle StreamDeck button presses
        StatusText = $"StreamDeck button {e.Index} pressed";
    }

    private void OnStreamDeckFaderChanged(object? sender, StreamDeckEventArgs e)
    {
        // Handle StreamDeck fader changes
        // Could control master intensity, manual lighting controls, etc.
    }

    private void OnStreamDeckEncoderChanged(object? sender, StreamDeckEventArgs e)
    {
        // Handle StreamDeck encoder changes
        // Could control fine lighting adjustments
    }

    #endregion

    public void Dispose()
    {
        // Unsubscribe from events
        _cueSheetService.CueTriggered -= OnCueTriggered;
        _cueSheetService.CueCompleted -= OnCueCompleted;
        _cueSheetService.ShowStateChanged -= OnShowStateChanged;

        _dmxService.UniverseConnected -= OnUniverseConnected;
        _dmxService.UniverseDisconnected -= OnUniverseDisconnected;

        _streamDeckService.ButtonPressed -= OnStreamDeckButtonPressed;
        _streamDeckService.FaderChanged -= OnStreamDeckFaderChanged;
        _streamDeckService.EncoderChanged -= OnStreamDeckEncoderChanged;
    }
}