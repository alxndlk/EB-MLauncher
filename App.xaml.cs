using System.Windows;
using System;
using System.Windows.Threading;

namespace Minecraft_Launcher_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var splash = new SplashWindow();
            splash.Show();
        }
    }
}

