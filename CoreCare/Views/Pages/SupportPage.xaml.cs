using System.Windows;
using System.Windows.Controls;
using CoreCare.Views.Components;
using CoreCare.ViewModels;

namespace CoreCare.Views.Pages
{
    public partial class SupportPage : Page
    {
        public SupportPage()
        {
            InitializeComponent();
            DataContext = new SupportPageViewModel();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new HeroSection());
        }
    }
}
