using SevenZip.Compression.LZ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Minecraft_Launcher_WPF
{
    /// <summary>
    /// Логика взаимодействия для OptionControl.xaml
    /// </summary>
    public partial class OptionControl : UserControl
    {
        string gameDirectory = System.IO.Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), ".xlauncher-demo");
        public event EventHandler CloseRequested;

        public OptionControl()
        {
            InitializeComponent();

            ulong totalMemoryInBytes = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
            ulong totalMemoryInMB = totalMemoryInBytes / (1024 * 1024);
            openDirectoryButton.Content = GetShortPath(gameDirectory);
            ramSlider.Maximum = totalMemoryInMB;

            ramLabel.Content = $"{(ramSlider.Value / 1024):F1}G";

            ramSlider.ValueChanged += RamSlider_ValueChanged;
            

        }

        private string GetShortPath(string path, int maxLength = 50)
        {
            if (path.Length > maxLength)
            {
                return path.Substring(0, maxLength) + "...";
            }
            return path;
        }

        private void RamSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ramLabel.Content = $"{(ramSlider.Value / 1024):F1}G";
        }
        private void Image_CloseWindow(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Hidden; 
        }

        private void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(gameDirectory))
                {
                    Process.Start("explorer.exe", gameDirectory);
                }
                else
                {
                    MessageBox.Show("Папка игры не найдена.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии директории: {ex.Message}");
            }
        }
    }
}
