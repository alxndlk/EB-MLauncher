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

            // Создание и показ загрузочного окна
            var splash = new SplashWindow();
            splash.Show();

            // Таймер для симуляции загрузки
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1) // Интервал обновления
            };

            timer.Tick += (sender, args) =>
            {
                if (splash.splashProgressBar.Value < splash.splashProgressBar.Maximum)
                {
                    splash.splashProgressBar.Value += 1; // Увеличиваем значение прогресс-бара
                }
                else
                {
                    // Открываем основное окно после загрузки
                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    timer.Stop(); // Останавливаем таймер
                    splash.Close(); // Закрываем загрузочное окно
                }
            };

            timer.Start();
        }
    }
}

