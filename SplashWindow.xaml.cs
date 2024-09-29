using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Minecraft_Launcher_WPF
{
    public partial class SplashWindow : Window
    {
        private string currentVersion = "1.0.0";
        private string versionFileUrl = "https://api.github.com/repos/alxndlk/MLauncher/contents/version.txt";
        private string latestReleaseUrl = "https://api.github.com/repos/alxndlk/MLauncher/releases/latest";

        public SplashWindow()
        {
            InitializeComponent();
            CheckLauncherVersion();
        }

        private async void CheckLauncherVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MinecraftLauncher");

                try
                {
                    // Проверяем последнюю версию
                    var response = await client.GetStringAsync(versionFileUrl);
                    dynamic versionInfo = JsonConvert.DeserializeObject(response);
                    string latestVersion = versionInfo.content;

                    latestVersion = Encoding.UTF8.GetString(Convert.FromBase64String(latestVersion)).Trim();

                    if (string.Compare(currentVersion, latestVersion) < 0)
                    {
                        newVersionText.Content = "Доступна новая версия лаунчера. Обновление...";

                        // Скачиваем и обновляем лаунчер
                        await DownloadAndInstallLatestVersion();
                    }
                    else
                    {
                        newVersionText.Content = "Лаунчер обновлён до последней версии.";
                        OpenMainWindow();
                    }
                }
                catch (Exception ex)
                {
                    newVersionText.Content = $"Произошла ошибка при проверке версии: {ex.Message}";
                    OpenMainWindow();
                }
            }
        }

        private async Task DownloadAndInstallLatestVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MinecraftLauncher");

                try
                {
                    // Получаем последнюю версию релиза
                    var releaseResponse = await client.GetStringAsync(latestReleaseUrl);
                    dynamic releaseInfo = JsonConvert.DeserializeObject(releaseResponse);
                    string downloadUrl = releaseInfo.zipball_url;

                    // Скачиваем архив с обновлением
                    var zipFilePath = Path.Combine(Environment.CurrentDirectory, "update.zip");
                    var zipData = await client.GetByteArrayAsync(downloadUrl);

                    // Сохраняем архив на диск
                    await File.WriteAllBytesAsync(zipFilePath, zipData);

                    // Распаковываем архив в текущую директорию
                    var extractPath = Environment.CurrentDirectory;
                    ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);

                    // Удаляем временный архив
                    File.Delete(zipFilePath);

                    // Перезапускаем приложение с новыми файлами
                    MessageBox.Show("Лаунчер обновлён. Перезапуск...");
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке обновления: {ex.Message}");
                    OpenMainWindow();
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
