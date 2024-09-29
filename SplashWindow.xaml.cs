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
        public SplashWindow()
        {
            InitializeComponent();
            CheckLauncherVersion();
        }

        private async void CheckLauncherVersion()
        {
            string currentVersion = "1.2.0"; // Текущая версия лаунчера
            string latestVersionUrl = "https://api.github.com/repos/alxndlk/MLauncher/contents/version.txt";
            string downloadUrl = "https://github.com/alxndlk/MLauncher/releases/latest/download/Minecraft-Launcher-WPF.zip"; // Обновите путь к скачиванию при необходимости

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MinecraftLauncher");

                try
                {
                    // Получаем содержимое version.txt
                    var response = await client.GetStringAsync(latestVersionUrl);
                    dynamic versionInfo = JsonConvert.DeserializeObject(response);
                    string latestVersion = versionInfo.content;

                    // Декодируем версию из Base64
                    latestVersion = Encoding.UTF8.GetString(Convert.FromBase64String(latestVersion)).Trim();

                    if (string.Compare(currentVersion, latestVersion) < 0)
                    {
                        newVersionText.Content = "Доступна новая версия лаунчера. Обновление...";
                        await UpdateLauncher(downloadUrl);
                    }
                    else
                    {
                        newVersionText.Content = "Лаунчер обновлён до последней версии.";
                        await Task.Delay(2000);
                        OpenMainWindow();
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    newVersionText.Content = $"Ошибка HTTP: {httpEx.Message}";
                }
                catch (Exception ex)
                {
                    newVersionText.Content = $"Произошла ошибка при проверке версии: {ex.Message}";
                }
            }
        }

        private async Task UpdateLauncher(string downloadUrl)
        {
            splashProgressBar.Visibility = Visibility.Visible;
            string tempPath = Path.Combine(Path.GetTempPath(), "launcher_update.zip");
            string extractPath = Path.Combine(Environment.CurrentDirectory, "update_temp");

            try
            {
                // Скачиваем архив с обновлением
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    // Получаем общее количество байт
                    long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    long receivedBytes = 0;

                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fs.WriteAsync(buffer, 0, bytesRead);
                            receivedBytes += bytesRead;

                            // Обновляем прогресс-бар
                            if (totalBytes > 0)
                            {
                                splashProgressBar.Value = (int)((receivedBytes * 100) / totalBytes);
                            }
                        }
                    }
                }

                // Проверяем, был ли загружен файл
                if (!File.Exists(tempPath) || new FileInfo(tempPath).Length == 0)
                {
                    throw new Exception("Файл обновления не был загружен корректно.");
                }

                // Распаковываем архив
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }

                ZipFile.ExtractToDirectory(tempPath, extractPath);

                // Заменяем текущие файлы лаунчера новыми
                CopyFilesRecursively(new DirectoryInfo(extractPath), new DirectoryInfo(Environment.CurrentDirectory));

                newVersionText.Content = "Лаунчер успешно обновлён. Перезапустите приложение.";
            }
            catch (Exception ex)
            {
                newVersionText.Content = $"Ошибка при обновлении: {ex.Message}";
            }
            finally
            {
                // Удаляем временные файлы
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
            }
        }

        private void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            // Сначала создаем все подкаталоги в целевой директории
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                DirectoryInfo targetSubdir = target.CreateSubdirectory(dir.Name);
                CopyFilesRecursively(dir, targetSubdir);
            }

            // Копируем все файлы из исходной директории в целевую
            foreach (FileInfo file in source.GetFiles())
            {
                string targetFilePath = Path.Combine(target.FullName, file.Name);
                file.CopyTo(targetFilePath, true); // true для перезаписи существующих файлов

                // Логируем процесс копирования
                Console.WriteLine($"Копируем {file.FullName} в {targetFilePath}");
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
