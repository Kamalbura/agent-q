using System;
using System.Windows;
using Assistant.Core.Services;
using Assistant.Core.Services.Vision;
using Assistant.Executor;
using Assistant.Executor.Abstractions;
using Assistant.Executor.Input;
using Assistant.Executor.Planning;
using Assistant.Executor.Process;
using Assistant.Executor.Services.Context;
using Assistant.Executor.Services.Ocr;
using Assistant.Executor.Services.Safety;
using Assistant.Executor.Services.ScreenCapture;
using Assistant.Executor.Services.UiAutomation;
using Assistant.Executor.Services.Vision;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Assistant.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        ConfigureLogging();

        // Global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
        {
            try
            {
                Log.Fatal(ev.ExceptionObject as Exception, "Unhandled domain exception");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        };

        DispatcherUnhandledException += (s, ev) =>
        {
            Log.Error(ev.Exception, "Unhandled UI dispatcher exception");
            ev.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, ev) =>
        {
            Log.Error(ev.Exception, "Unobserved task exception");
            ev.SetObserved();
        };

        ShutdownMode = ShutdownMode.OnMainWindowClose;

        var services = new ServiceCollection();
        ConfigureServices(services);

        try
        {
            _serviceProvider = services.BuildServiceProvider();
            var window = _serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }
        catch (Exception ex)
        {
            // If DI or window creation fails, log and show a simple message then exit.
            Log.Fatal(ex, "Failed to start application");
            MessageBox.Show($"Failed to start Astra: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(new DiagnosticsSink())
            .CreateLogger();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Log.Logger);
        services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
        services.AddSingleton<IOcrService, NullOcrService>();
        services.AddSingleton<IUiAutomationService, UiAutomationService>();
        services.AddSingleton<IScreenContextCollector, ScreenContextCollector>();
        services.AddSingleton<IVisionLlmAdapter, VisionLlmAdapterStub>();
        services.AddSingleton<ISafetyLayer, SimpleSafetyLayer>();
        services.AddSingleton<ILlmPlanGenerator, LlmPlanGenerator>();
        services.AddSingleton<ITextEntrySimulator, InputSimulatorTextEntry>();
        services.AddSingleton<IProcessLauncher, ProcessLauncher>();
        services.AddSingleton<IActionExecutor, ActionExecutor>();

        services.AddSingleton<MainWindow>();
    }

    private sealed class DiagnosticsSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            System.Diagnostics.Debug.WriteLine(logEvent.RenderMessage(null));
        }
    }
}

