using System.Windows;
using System.Windows.Controls;
using CoreCare.ViewModels;
using CoreCare.Services;
using CoreCare.Views.Pages;

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
            App.IsUserLoggedIn = false;
            App.CurrentUsername = "Invitado";
            App.IsPremium = false;
            SessionService.SignOut();
            
            var adminWindow = Window.GetWindow(this);

            var mainWindow = new CoreCare.MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            adminWindow?.Close();
        }
    }
}

