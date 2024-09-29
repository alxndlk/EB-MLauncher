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
            CheckLauncherVersion(); 
        }

        private async void CheckLauncherVersion()
        {
            string currentVersion = "1.0.0";
            string latestVersionUrl = "https://api.github.com/repos/alxndlk/MLauncher/contents/version.txt";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MinecraftLauncher");
                try
                {
                    var response = await client.GetStringAsync(latestVersionUrl);
                    dynamic versionInfo = JsonConvert.DeserializeObject(response);
                    string latestVersion = versionInfo.content; 

                    latestVersion = Encoding.UTF8.GetString(Convert.FromBase64String(latestVersion));

                    if (string.Compare(currentVersion, latestVersion) < 0)
                    {
                        MessageBox.Show("Доступна новая версия лаунчера. Пожалуйста, обновите.");
                    }
                    else
                    {
                        MessageBox.Show("Лаунчер обновлён до последней версии.");
                        Application.Current.Shutdown();
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
            this.Hide();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
