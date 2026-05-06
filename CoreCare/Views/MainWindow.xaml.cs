using System.Windows;
using CoreCare.Views.Components; // Importante para que encuentre el HeroSection

namespace CoreCare
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // PASO 1: Cargar la pantalla de bienvenida directamente
            MainContentArea.Content = new HeroSection();
        }
    }
}