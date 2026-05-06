using System.Windows;
using System.Windows.Controls;
using CoreCare.Views.Pages;

namespace CoreCare.Views.Components
{
    public partial class HeroSection : UserControl
    {
        public HeroSection()
        {
            InitializeComponent();
        }

        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            // Buscamos la ventana MainWindow
            var parentWindow = Window.GetWindow(this) as MainWindow;

            if (parentWindow != null)
            {
                // Ahora que el nombre coincide (MainContentArea), esto no dará error
                parentWindow.MainContentArea.Content = new MainDashboardPage();
            }
        }
    }
}