using System.Windows;
using System.Windows.Controls;
using CoreCare.Services;
using CoreCare.ViewModels;

namespace CoreCare.Views
{
    public partial class AdminPanel : UserControl
    {
        public AdminPanel()
        {
            InitializeComponent();

            // Always use the admin-specific VM for this view.
            DataContext = new AdminPanelViewModel();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "¿Cerrar sesión actual?",
                "CoreCare",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SessionService.SignOut();

            // Show the login window and close this main window so only one window remains.
            var loginWindow = new LoginWindow();
            // Set as current main window so the application's MainWindow reference is valid
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            // Close the main window
            Window.GetWindow(this)?.Close();
        }
    }
}
