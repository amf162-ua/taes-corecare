using System.Windows.Controls;
using CoreCare.ViewModels;

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
    }
}
