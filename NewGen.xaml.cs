using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ProcessBuilder;
using CmlLib.Core;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Minecraft_Launcher_WPF
{
    /// <summary>
    /// Логика взаимодействия для ContentControl.xaml
    /// </summary>
    public partial class NewGen : UserControl
    {
        private MainWindow mainWindow;
        private string gitHubToken;

        public NewGen(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            LoadConfig();
        }

        private void LoadConfig()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, @"..\..\..\config.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                dynamic config = JsonConvert.DeserializeObject(json);
                gitHubToken = config.AccessToken;
            } else
            {
                MessageBox.Show("Нет доступа к ключу авторизаци...");
            }
        }

        private async void Button_Play(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            installProgress.Visibility = Visibility.Visible;

            var pathNEWGEN = new MinecraftPath(Environment.GetEnvironmentVariable("APPDATA") + "/.xlauncher-demo" + "/NewGEN");
            var launcher = new MinecraftPath(Environment.GetEnvironmentVariable("APPDATA") + "/.xlauncher-demo");
            var modsPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".xlauncher-demo", "NewGEN", "mods");
            var configPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".xlauncher-demo", "NewGEN", "config");
            var resourcepacksPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".xlauncher-demo", "NewGEN", "resourcepacks");
            var shaderpacksPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".xlauncher-demo", "NewGEN", "shaderpacks");

            // Проверяем и создаём папки, если их нет
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
            }

            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            if (!Directory.Exists(resourcepacksPath))
            {
                Directory.CreateDirectory(resourcepacksPath);
            }

            if (!Directory.Exists(shaderpacksPath))
            {
                Directory.CreateDirectory(shaderpacksPath);
            }

            var LoadingMods = new MinecraftPath(modsPath);
            var LoadingConfig = new MinecraftPath(configPath);
            var LoadingResourcepacks = new MinecraftPath(resourcepacksPath);
            var LoadingShaderpacks = new MinecraftPath(shaderpacksPath);

            await SetupMinecraftModsAndConfig(pathNEWGEN, LoadingMods, LoadingConfig, LoadingResourcepacks, LoadingShaderpacks);

            await listVersion("123", pathNEWGEN);

            ApplySuccessStyles();
        }

        private async Task SetupMinecraftModsAndConfig(MinecraftPath pathNEWGEN, MinecraftPath modsPath, MinecraftPath configPath, MinecraftPath resourcepacksPath, MinecraftPath shaderpacksPath)
        {
            await DownloadRepositoryFiles("https://api.github.com/repos/alxndlk/epohablokov-newgen-mods/contents/", modsPath.BasePath, "Установка Аркании: Моды");

            await DownloadRepositoryFiles("https://api.github.com/repos/alxndlk/epohablokov-newgen-config/contents/", configPath.BasePath, "Установка Аркании: Конфиги");

            await DownloadRepositoryFiles("https://api.github.com/repos/alxndlk/epohablokov-newgen-resourcepacks/contents/", resourcepacksPath.BasePath, "Установка Аркании: Ресурсы");

            await DownloadRepositoryFiles("https://api.github.com/repos/alxndlk/epohablokov-newgen-shaderpacks/contents/", shaderpacksPath.BasePath, "Установка Аркании: Шейдеры");

            await DownloadRepositoryFiles("https://api.github.com/repos/alxndlk/epohablokov-newgen-user/contents/", pathNEWGEN.BasePath, "Установка Аркании: Пользователь");

            startButton.IsEnabled = true;
        }


        private async Task DownloadRepositoryFiles(string repositoryUrl, string destinationPath, string statusLabelPath)
        {
            statusLabel.Content = statusLabelPath;
            startButton.Background = (Brush)new BrushConverter().ConvertFromString("#FF01568C");
            startButton.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF0090EC");
            startText.Content = "УСТАНОВКА...";

            RotateTransform rotateTransform = new RotateTransform();
            LoadingImage.RenderTransform = rotateTransform;
            LoadingImage.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation rotationAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(1)));
            rotationAnimation.RepeatBehavior = RepeatBehavior.Forever;

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);

            try
            {
                string apiUrl = repositoryUrl;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "CmlLib-Minecraft-Launcher");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", gitHubToken);

                    var response = await client.GetStringAsync(apiUrl);
                    dynamic items = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                    foreach (var item in items)
                    {
                        string itemName = item.name.ToString();
                        string itemType = item.type.ToString();  
                        string itemPath = Path.Combine(destinationPath, itemName);

                        if (itemType == "file")
                        {
                            string fileDownloadUrl = item.download_url.ToString();

                            if (File.Exists(itemPath))
                            {
                                continue;
                            }

                            var fileResponse = await client.GetAsync(fileDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                            if (fileResponse.IsSuccessStatusCode)
                            {
                                var totalBytes = fileResponse.Content.Headers.ContentLength ?? -1L;

                                string fileSize = totalBytes >= 1024 * 1024
                                    ? $"{(totalBytes / (1024 * 1024))} МБ"
                                    : $"{(totalBytes / 1024)} КБ";

                                var buffer = new byte[8192];
                                int bytesRead;
                                long totalRead = 0;

                                var startTime = DateTime.Now;

                                using (var contentStream = await fileResponse.Content.ReadAsStreamAsync())
                                using (var fs = new FileStream(itemPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true))
                                {
                                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                    {
                                        progressBar.Visibility = Visibility.Visible;
                                        await fs.WriteAsync(buffer, 0, bytesRead);
                                        totalRead += bytesRead;
                                        modNameText.Text = itemName;

                                        var elapsedTime = (DateTime.Now - startTime).TotalSeconds;
                                        var downloadSpeed = totalRead / elapsedTime;

                                        progressBar.Value = (double)totalRead / totalBytes * 100;
                                        speedLabel.Content = $"{(totalRead >= 1024 * 1024 ? $"{(totalRead / (1024 * 1024))} МБ" : $"{(totalRead / 1024)} КБ")} / {fileSize}";
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Не удалось скачать файл: {itemName}");
                            }
                        }
                        else if (itemType == "dir")
                        {
                            if (!Directory.Exists(itemPath))
                            {
                                Directory.CreateDirectory(itemPath);
                            }

                            string subDirectoryUrl = item.url.ToString();
                            await DownloadRepositoryFiles(subDirectoryUrl, itemPath, statusLabelPath); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null); 
            }
        }

        private async Task listVersion(string username, MinecraftPath pathNEWGEN)
        {
            statusLabel.Content = "Установка Аркании";
            var mcVersion = "1.20.1";
            var forgeVersion = "47.2.32";
            var launcher = new MinecraftLauncher(pathNEWGEN);
            var forge = new ForgeInstaller(launcher);
            var version_name = await forge.Install(mcVersion, forgeVersion);

            launcher.FileProgressChanged += (sender, args) =>
            {
                modNameText.Text = "Установка Minecraft и Forge...";
                progressBar.Visibility = Visibility.Visible;
                progressBar.Maximum = args.TotalTasks;
                progressBar.Value = args.ProgressedTasks;
            };

            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 8196,
                Session = MSession.CreateOfflineSession("123"),
            };

            var process = await launcher.InstallAndBuildProcessAsync(version_name, launchOption);
            process.Start();
            startButton.IsEnabled = true;
        }

        private void ApplySuccessStyles()
        {
            LoadingImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resourses/cheked.png"));
            LoadingImage.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            modNameText.Text = "Все файлы установлены успешно.";
            speedLabel.Visibility = Visibility.Hidden;
            startButton.Background = (Brush)new BrushConverter().ConvertFromString("#FF018C44");
            startButton.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF00CA60");
            startText.Content = "ЗАПУСК...";
        }
    }
}