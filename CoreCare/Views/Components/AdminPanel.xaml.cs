using System.Windows;
using System.Windows.Controls;
using CoreCare.ViewModels;
using CoreCare.Services;

namespace CoreCare.Views.Components
{
    public partial class AdminPanel : UserControl
    {
        private AdminPanelViewModel _viewModel;

        public AdminPanel()
        {
            InitializeComponent();
            _viewModel = new AdminPanelViewModel();
            DataContext = _viewModel;
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel?.Dispose();
        }

        private void BtnCloseAdmin_Click(object sender, RoutedEventArgs e)
        {
            // Sign out the user
            SessionService.SignOut();
            
            // Close the admin window
            var adminWindow = Window.GetWindow(this);
            
            // Show LoginWindow before closing AdminWindow
            var loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            
            // Close admin window
            adminWindow?.Close();
        }
    }
}

