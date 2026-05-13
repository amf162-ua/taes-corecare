using System.Windows;
using System.Windows.Controls;
using CoreCare.Views.Pages;
using CoreCare.Views.Modals;
using CoreCare.Views;

namespace CoreCare.Views.Components
{
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            ActualizarInterfaz();
        }

        public void ActualizarInterfaz()
        {
            if (App.IsUserLoggedIn)
            {
                GuestPanel.Visibility = Visibility.Collapsed;
                UserPanel.Visibility = Visibility.Visible;
                BtnPremium.Visibility = Visibility.Visible; // Mostramos premium
                BtnSupport.Visibility = Visibility.Visible; // Mostrar soporte para usuarios logueados
                TxtUsername.Text = App.CurrentUsername.ToUpper();
            }
            else
            {
                GuestPanel.Visibility = Visibility.Visible;
                UserPanel.Visibility = Visibility.Collapsed;
                BtnPremium.Visibility = Visibility.Collapsed; // Ocultamos premium
                BtnSupport.Visibility = Visibility.Collapsed; // Ocultar soporte para no logueados
            }
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new MainDashboardPage());
        }

        private void BtnBenchmarks_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new BenchmarksPage()); // [cite: 1]
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new HistoryPage());
        }

        private void BtnSupport_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new SupportPage());
        }

        private void BtnArranque_Click(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new ArranquePage());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWin = new LoginWindow();
            loginWin.Owner = Window.GetWindow(this);
            if (loginWin.ShowDialog() == true) ActualizarInterfaz();
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            // PASAMOS LOS ARGUMENTOS: Nombre y Estado de Login
            var profileWin = new UserProfileWindow(App.CurrentUsername, App.IsUserLoggedIn);
            profileWin.Owner = Window.GetWindow(this);
            profileWin.ShowDialog();
            ActualizarInterfaz();
        }

        private void BtnGoPremium_Click(object sender, RoutedEventArgs e)
        {
            // 1. Abrimos primero la ventana de beneficios (PremiumWindow)
            var premiumWin = new PremiumWindow();
            premiumWin.Owner = Window.GetWindow(this);

            // Si el usuario pulsa "Actualizar" en esa ventana, ShowDialog() devuelve true
            if (premiumWin.ShowDialog() == true)
            {
                // 2. Si aceptó, abrimos inmediatamente la ventana de pago (PaymentWindow)
                var paymentWin = new PaymentWindow();
                paymentWin.Owner = Window.GetWindow(this);

                // Si el pago se procesa correctamente
                if (paymentWin.ShowDialog() == true)
                {
                    App.IsPremium = true; // Activamos el premium global
                    ActualizarInterfaz(); // Refrescamos el header para ocultar el botón
                    MessageBox.Show("¡Suscripción activada con éxito!", "Core Care Premium");
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.IsUserLoggedIn = false;
            App.CurrentUsername = "Invitado";
            ActualizarInterfaz();
            var win = Window.GetWindow(this) as MainWindow;
            win?.MainContentArea.Navigate(new MainDashboardPage());
        }
    }
}