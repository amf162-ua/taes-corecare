using System.Windows;
using System.Windows.Media;
using CoreCare.Models;

namespace CoreCare.Views.Modals
{
    public partial class UserProfileWindow : Window
    {
        private HardwareProfile _hardwareData;
        private HardwareProfile _backupData;
        private bool _isEditing = false;

        public UserProfileWindow(string userName, bool isPremium)
        {
            InitializeComponent();

            // Configurar datos de usuario
            TxtUserName.Text = userName;

            // Si es premium, mostramos la medalla y ocultamos la sección de compra
            if (isPremium)
            {
                PremiumBadge.Visibility = Visibility.Visible;
                PremiumPurchaseSection.Visibility = Visibility.Collapsed;
            }
            else
            {
                PremiumBadge.Visibility = Visibility.Collapsed;
                PremiumPurchaseSection.Visibility = Visibility.Visible;
            }

            // Inicializar datos de hardware (esto vendría de una DB normalmente)
            _hardwareData = new HardwareProfile
            {
                Cpu = "Intel Core i9-13900K",
                Gpu = "NVIDIA RTX 4090",
                Ram = "32GB DDR5 6000MHz",
                Storage = "2TB NVMe SSD",
                Motherboard = "ASUS ROG Z790"
            };

            HardwareFormPanel.DataContext = _hardwareData;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            // Cerramos o minimizamos el perfil si quieres, pero lo mejor es abrirlo encima
            var premiumWin = new PremiumWindow();
            premiumWin.Owner = this; // Esta ventana es la dueña ahora

            if (premiumWin.ShowDialog() == true)
            {
                var paymentWin = new PaymentWindow();
                paymentWin.Owner = this;

                if (paymentWin.ShowDialog() == true)
                {
                    App.IsPremium = true;

                    // Actualizamos visualmente el perfil sin cerrarlo
                    PremiumBadge.Visibility = Visibility.Visible;
                    PremiumPurchaseSection.Visibility = Visibility.Collapsed;

                    MessageBox.Show("¡Bienvenido a Premium!", "Éxito");
                }
            }
        }

        private void BtnEditSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                _isEditing = true;
                _backupData = new HardwareProfile
                {
                    Cpu = _hardwareData.Cpu,
                    Gpu = _hardwareData.Gpu,
                    Ram = _hardwareData.Ram,
                    Storage = _hardwareData.Storage,
                    Motherboard = _hardwareData.Motherboard
                };

                HardwareFormPanel.IsEnabled = true;
                BtnEditSave.Content = "GUARDAR";
                BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34));
                BtnCancelEdit.Visibility = Visibility.Visible;
            }
            else
            {
                _isEditing = false;
                HardwareFormPanel.IsEnabled = false;
                BtnEditSave.Content = "EDITAR";
                BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(0, 139, 139));
                BtnCancelEdit.Visibility = Visibility.Collapsed;

                MessageBox.Show("Perfil de hardware actualizado correctamente.", "Guardado");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _hardwareData.Cpu = _backupData.Cpu;
            _hardwareData.Gpu = _backupData.Gpu;
            _hardwareData.Ram = _backupData.Ram;
            _hardwareData.Storage = _backupData.Storage;
            _hardwareData.Motherboard = _backupData.Motherboard;

            _isEditing = false;
            HardwareFormPanel.IsEnabled = false;
            BtnEditSave.Content = "EDITAR";
            BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(0, 139, 139));
            BtnCancelEdit.Visibility = Visibility.Collapsed;

            // Refrescar binding
            HardwareFormPanel.DataContext = null;
            HardwareFormPanel.DataContext = _hardwareData;
        }
    }
}