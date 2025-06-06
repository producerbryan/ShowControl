using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShowControl.Services;
using ShowControl.ViewModels;
using System;
using System.Windows;

namespace ShowControl;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core Services
                services.AddSingleton<ICueSheetService, CueSheetService>();
                services.AddSingleton<IAudioService, AudioService>();
                services.AddSingleton<IVideoService, VideoService>();
                services.AddSingleton<IDmxService, DmxService>();
                services.AddSingleton<IMidiService, MidiService>();
                services.AddSingleton<IStreamDeckService, StreamDeckService>();
                services.AddSingleton<IProjectService, ProjectService>();
                services.AddSingleton<ITimelineService, TimelineService>();

                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<CueSheetViewModel>();
                services.AddTransient<TimelineViewModel>();
                services.AddTransient<DmxControlViewModel>();
                services.AddTransient<AudioControlViewModel>();
            })
            .Build();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host?.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
    }
}