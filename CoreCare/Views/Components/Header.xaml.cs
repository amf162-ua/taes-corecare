using System.Windows;
using System.Windows.Controls;

namespace CoreCare.Views.Components
{
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.MainFrame.Navigate(new Pages.HomePage());
        }

        private void BtnBenchmarks_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.MainFrame.Navigate(new Pages.BenchmarksPage());
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.MainFrame.Navigate(new Pages.HistoryPage());
        }

        // 👇 ¡AÑADIDO! Navegación para la página de Reportes
        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.MainFrame.Navigate(new Pages.ReportsPage());
        }

        // 👇 ¡AÑADIDO! Navegación para el Gestor de Procesos
        private void BtnProcesses_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.MainFrame.Navigate(new Pages.Processes());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Aquí es donde abrimos el LoginWindow
            var loginWin = new Modals.LoginWindow();
            loginWin.Owner = Window.GetWindow(this);
            loginWin.ShowDialog();
        }
    }
}