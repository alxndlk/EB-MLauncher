using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Minecraft_Launcher_WPF
{
    public partial class LoginControl : UserControl
    {
        public event EventHandler Authenticated;

        private static readonly string connectionString = "mongodb+srv://livik9903:eOZG241x0y27Wyzd@userdatabase.qelze.mongodb.net/?retryWrites=true&w=majority&appName=UserDataBase";
        private static readonly MongoClient client = new MongoClient(connectionString);
        private static readonly IMongoDatabase database = client.GetDatabase("UserDB");

        public string userName;
        public static bool isAuthenticated = false;


        private readonly string credentialsFilePath = "user_credentials.txt";

        public LoginControl()
        {
            InitializeComponent();
            LoadCredentials();
        }


        public void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string usernameOrEmail = txtUsername.Text;
            string password = txtPassword.Password;

            var collection = database.GetCollection<BsonDocument>("user-data");

            var usernameFilter = Builders<BsonDocument>.Filter.Eq("username", usernameOrEmail);
            var emailFilter = Builders<BsonDocument>.Filter.Eq("email", usernameOrEmail);
            var passwordFilter = Builders<BsonDocument>.Filter.Eq("password", password);

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Or(usernameFilter, emailFilter),
                passwordFilter
            );

            var foundUser = collection.Find(filter).FirstOrDefault();

            if (foundUser != null)
            {
                isAuthenticated = true;
                userName = foundUser.GetValue("username", "").AsString;

                // Сохранение данных в файл
                SaveCredentials(usernameOrEmail, password);

                Authenticated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                isAuthenticated = false;
                MessageBox.Show("Invalid username or password.");
            }
        }

        // Сохранение учетных данных
        private void SaveCredentials(string usernameOrEmail, string password)
        {
            using (StreamWriter writer = new StreamWriter(credentialsFilePath))
            {
                writer.WriteLine(usernameOrEmail);
                writer.WriteLine(password);
            }
        }

        // Загрузка учетных данных
        private void LoadCredentials()
        {
            if (File.Exists(credentialsFilePath))
            {
                var lines = File.ReadAllLines(credentialsFilePath);
                if (lines.Length == 2)
                {
                    txtUsername.Text = lines[0]; // Автоматически заполняем поле имени пользователя
                    txtPassword.Password = lines[1]; // Автоматически заполняем поле пароля
                }
            }
        }
    }
}
