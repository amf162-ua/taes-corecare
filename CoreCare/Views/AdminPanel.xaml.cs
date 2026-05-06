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

        private void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminPanelViewModel vm)
            {
                // Reload all users (LoadAllUsers is private, so we'll refresh by clearing filters)
                vm.ClearUserFiltersCommand.Execute(null);
            }
        }

        private void RoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AdminPanelViewModel vm && sender is ComboBox cb)
            {
                var selectedItem = cb.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    var role = selectedItem.Content?.ToString();
                    vm.RoleFilter = role switch
                    {
                        "Cliente" => Models.UserRole.Cliente,
                        "Administrador" => Models.UserRole.Administrador,
                        _ => null
                    };
                }
            }
        }
    }
}
