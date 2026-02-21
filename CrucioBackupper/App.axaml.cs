using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CrucioBackupper.ViewModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrucioBackupper;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ThemeInfoViewModel.Instance.Init();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            if (ResolveDignPathFromArgs(desktop.Args) is var dignPath && dignPath != null)
            {
                // Show dign reader after the main window is opened
                void handler(object? sender, EventArgs e)
                {
                    desktop.MainWindow.Opened -= handler;
                    try
                    {
                        var dignReader = new DignReader(dignPath);
                        dignReader.Show();
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception, "打开对话小说文件失败");
                    }
                }
                desktop.MainWindow.Opened += handler;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string? ResolveDignPathFromArgs(IReadOnlyList<string>? args)
    {
        if (args == null || args.Count == 0)
        {
            return null;
        }
        foreach (var rawArg in args)
        {

            try
            {
                var path = Path.GetFullPath(rawArg);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            catch
            {
                // Ignore invalid argument path and continue checking other arguments.
            }
        }
        return null;
    }
}
