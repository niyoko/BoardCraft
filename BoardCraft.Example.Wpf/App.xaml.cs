namespace BoardCraft.Example.Wpf
{
    using System.Diagnostics;
    using System.Windows;
    using NLog;
    using ViewModels;
    using Views;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Debug.Listeners.Add(new NLogTraceListener());

            MainWindow = new MainWindow();
            MainWindow.DataContext = new MainWindowViewModel();
            MainWindow.Show();
        }
    }
}