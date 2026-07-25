using System.Windows;
using KB.Jarvis.App.Core;

namespace KB.Jarvis.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            JarvisLog.Error("Unhandled UI exception", args.Exception);
            MessageBox.Show(
                "KB Jarvis encountered an unexpected error. The incident has been recorded in the local log.",
                Identity.ProductName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                JarvisLog.Error("Unhandled process exception", exception);
            }
        };

        base.OnStartup(e);
        var window = new MainWindow();
        MainWindow = window;
        window.InitializeV15();
        window.Show();
    }
}
