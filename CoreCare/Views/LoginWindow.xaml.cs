using System.Windows;
using CoreCare.Services;
using CoreCare.Models;

namespace CoreCare
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginStatusText.Text = string.Empty;

            var user = _authService.Login(LoginIdentifierBox.Text, LoginPasswordBox.Password);
            if (user == null)
            {
                LoginStatusText.Text = "Credenciales inválidas o usuario desactivado.";
                return;
            }

            SessionService.SignIn(user);
            
            // Update global app state
            App.IsUserLoggedIn = true;
            App.CurrentUsername = user.name ?? user.username;

            // Redirect admins to AdminPanel, regular users to MainWindow
            if (user.Role == UserRole.Administrador)
            {
                var adminWindow = new AdminWindow();
                Application.Current.MainWindow = adminWindow;
                adminWindow.Show();
            }
            else
            {
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            RegisterStatusText.Text = string.Empty;

            try
            {
                var user = _authService.RegisterClient(
                    RegisterNameBox.Text,
                    RegisterUsernameBox.Text,
                    RegisterEmailBox.Text,
                    RegisterPasswordBox.Password);

                SessionService.SignIn(user);
                
                // Update global app state
                App.IsUserLoggedIn = true;
                App.CurrentUsername = user.name ?? user.username;

                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                Close();
            }
            catch (System.Exception ex)
            {
                RegisterStatusText.Text = ex.GetBaseException().Message;
            }
        }
    }
}