using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.Installers;
using CmlLib.Core.ProcessBuilder;
using MongoDB.Bson;
using MongoDB.Driver;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Minecraft_Launcher_WPF
{
    public partial class MainWindow : Window
    {


        private bool isDragging = false;
        private Point clickPosition;
        public  UserControl currentWindow;
        public string AuthenticatedUserName { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            LoginControl.Authenticated += LoginControl_Authenticated;
            currentWindow = new NewGen(this);
            MainContentControl.Content = currentWindow;
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                clickPosition = e.GetPosition(this);
                CaptureMouse();
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(null);
                double offsetX = currentPosition.X - clickPosition.X;
                double offsetY = currentPosition.Y - clickPosition.Y;

                Left += offsetX;
                Top += offsetY;
            }
        }
        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
                ReleaseMouseCapture();
            }
        }

        private void LoginControl_Authenticated(object sender, EventArgs e)
        {
            LoginControl.Visibility = Visibility.Collapsed;
        }

        // Закрыть прилу

        private void Image_CloseWindow(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Свернуть прилу
        private void Image_CollapseWindow(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void PlayButton_Click(object sender, MouseButtonEventArgs e)
        {
            MainContentControl.Content = currentWindow;
        }

        // Открыть настройки

        private void SettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            OptionControl.Visibility = Visibility.Visible;
        }

        private void OnAuthenticated(object sender, EventArgs e)
        {
            var loginControl = sender as LoginControl;
            if (loginControl != null)
            {
                // Устанавливаем имя пользователя
                AuthenticatedUserName = loginControl.userName;

                // Переходим к контролу TMRPG, передавая имя пользователя
                var tmrpgControl = new NewGen(this);
                MainContentControl.Content = tmrpgControl;
            }
        }
    }
}