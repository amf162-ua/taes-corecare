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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Aquí es donde abriríamos el LoginWindow que estuvimos viendo
            var loginWin = new Modals.LoginWindow();
            loginWin.Owner = Window.GetWindow(this);
            loginWin.ShowDialog();
        }
    }
}