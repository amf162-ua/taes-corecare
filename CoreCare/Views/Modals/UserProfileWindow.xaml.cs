using System.Windows;
using System.Windows.Media;
using CoreCare.Models;

namespace CoreCare.Views.Modals
{
    public partial class UserProfileWindow : Window
    {
        private HardwareProfile _hardwareData;
        private HardwareProfile _backupData; // Para cancelar edición
        private bool _isEditing = false;

        public UserProfileWindow(string userName, bool isPremium)
        {
            InitializeComponent();

            // Configurar datos de usuario
            TxtUserName.Text = userName;
            if (isPremium)
            {
                PremiumBadge.Visibility = Visibility.Visible;
            }

            // Inicializar y bindear los datos de hardware
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

        private void BtnEditSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                // Entrar en modo edición
                _isEditing = true;

                // Guardar copia de seguridad por si cancelan
                _backupData = new HardwareProfile
                {
                    Cpu = _hardwareData.Cpu,
                    Gpu = _hardwareData.Gpu,
                    Ram = _hardwareData.Ram,
                    Storage = _hardwareData.Storage,
                    Motherboard = _hardwareData.Motherboard
                };

                // Cambiar UI
                HardwareFormPanel.IsEnabled = true;
                BtnEditSave.Content = "GUARDAR";
                BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Verde
                BtnCancelEdit.Visibility = Visibility.Visible;
            }
            else
            {
                // Guardar cambios
                _isEditing = false;
                HardwareFormPanel.IsEnabled = false;
                BtnEditSave.Content = "EDITAR";
                BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(0, 139, 139)); // Cian oscuro
                BtnCancelEdit.Visibility = Visibility.Collapsed;

                // Aquí podrías guardar _hardwareData en la base de datos
                MessageBox.Show("Perfil de hardware actualizado correctamente.", "Guardado");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Restaurar copia de seguridad
            _hardwareData.Cpu = _backupData.Cpu;
            _hardwareData.Gpu = _backupData.Gpu;
            _hardwareData.Ram = _backupData.Ram;
            _hardwareData.Storage = _backupData.Storage;
            _hardwareData.Motherboard = _backupData.Motherboard;

            // Salir de modo edición
            _isEditing = false;
            HardwareFormPanel.IsEnabled = false;
            BtnEditSave.Content = "EDITAR";
            BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(0, 139, 139));
            BtnCancelEdit.Visibility = Visibility.Collapsed;
        }
    }
}