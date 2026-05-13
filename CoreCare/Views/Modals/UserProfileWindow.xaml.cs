using System;
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

            // Configurar datos básicos del usuario
            TxtUserName.Text = userName;

            // Gestión visual del estado Premium
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

            // Cargar los componentes reales del sistema
            LoadRealHardwareData();
        }

        private void LoadRealHardwareData()
        {
            // Accedemos al servicio de monitorización
            var monitor = App.Monitor;
            monitor.UpdateHardware(); // Actualizamos sensores antes de leer

            _hardwareData = new HardwareProfile();

            // 1. Detección de CPU (Núcleos y Velocidad real)
            if (monitor.TryGetCpuClockGHz(out float clock, out _))
            {
                _hardwareData.Cpu = $"Intel/AMD ({monitor.Cores} Cores) @ {clock:F2} GHz";
            }

            // 2. Detección de RAM (Uso actual en GB)
            if (monitor.TryGetRamUsageGb(out float ramUsed, out _))
            {
                _hardwareData.Ram = $"{ramUsed:F1} GB en uso actual";
            }

            // 3. Detección de GPU (Carga y Temperatura si está disponible)
            if (monitor.TryGetGpuLoad(out float gLoad, out _) && monitor.TryGetGpuTemperature(out float gTemp, out _))
            {
                _hardwareData.Gpu = $"GPU Activa: {gLoad:F0}% Carga / {gTemp:F0}°C";
            }
            else
            {
                _hardwareData.Gpu = "GPU Detectada (Sin sensores de telemetría)";
            }

            // 4. Actividad del Almacenamiento
            if (monitor.TryGetDiskLoad(out float dLoad, out _))
            {
                _hardwareData.Storage = $"Actividad de Disco: {dLoad:F1}%";
            }

            // 5. Datos de Placa Base y Sistema (Uptime)
            _hardwareData.Motherboard = $"Sistema Operativo (Uptime: {monitor.GetUpTime()})";

            // Vinculamos los datos reales al formulario XAML
            HardwareFormPanel.DataContext = _hardwareData;
        }

        private void BtnEditSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                // Iniciar modo edición: creamos respaldo
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
                // Finalizar edición y guardar cambios
                _isEditing = false;
                HardwareFormPanel.IsEnabled = false;
                BtnEditSave.Content = "EDITAR";
                BtnEditSave.Background = new SolidColorBrush(Color.FromRgb(0, 139, 139)); // Cian
                BtnCancelEdit.Visibility = Visibility.Collapsed;

                MessageBox.Show("Perfil de hardware actualizado correctamente.", "Guardado");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Restaurar datos desde el respaldo
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

            // Refrescar el enlace de datos
            HardwareFormPanel.DataContext = null;
            HardwareFormPanel.DataContext = _hardwareData;
        }

        // ¡Aquí estaba el problema! He dejado solo esta versión que es más limpia
        private void BtnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            // Flujo de actualización a Premium
            var premiumWin = new PremiumWindow { Owner = this };

            if (premiumWin.ShowDialog() == true)
            {
                var paymentWin = new PaymentWindow { Owner = this };

                if (paymentWin.ShowDialog() == true)
                {
                    App.IsPremium = true;
                    PremiumBadge.Visibility = Visibility.Visible;
                    PremiumPurchaseSection.Visibility = Visibility.Collapsed;
                    MessageBox.Show("¡Bienvenido a Core Care Premium!", "Éxito");
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}