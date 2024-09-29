using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Minecraft_Launcher_WPF
{
    /// <summary>
    /// Логика взаимодействия для SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            CheckLauncherVersion(); // Вызов проверки версии при запуске окна
        }

        private async void CheckLauncherVersion()
        {
            string currentVersion = "1.0.0"; // Замените на вашу текущую версию
            string latestVersionUrl = "https://api.github.com/repos/ваш_пользователь/ваш_репозиторий/contents/version.txt"; // Укажите URL для файла версии

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MinecraftLauncher");
                try
                {
                    var response = await client.GetStringAsync(latestVersionUrl);
                    dynamic versionInfo = JsonConvert.DeserializeObject(response);
                    string latestVersion = versionInfo.content; // Получаем содержимое файла

                    // Декодируем содержимое в строку
                    latestVersion = Encoding.UTF8.GetString(Convert.FromBase64String(latestVersion));

                    if (string.Compare(currentVersion, latestVersion) < 0)
                    {
                        MessageBox.Show("Доступна новая версия лаунчера. Пожалуйста, обновите.");
                        // Можно завершить приложение или направить пользователя на страницу с обновлением
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        // Если версия актуальна, можно скрыть SplashScreen и запустить основной лаунчер
                        MessageBox.Show("Лаунчер обновлён до последней версии.");
                        OpenMainWindow(); // Открываем основное окно
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при проверке версии: {ex.Message}");
                }
            }
        }

        private void OpenMainWindow()
        {
            // Закрываем окно SplashScreen и открываем основное окно лаунчера
            this.Hide();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
