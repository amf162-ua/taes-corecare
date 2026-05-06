using System.Windows;
using CoreCare.Services;

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

            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
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