using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // 1. Declaración de los tres servicios
        private readonly HardwareMonitorService _hardwareService;
        private readonly ProcessService _processService;
        private readonly StartupService _startupService;

        [ObservableProperty]
        private string _cpuDisplay = "Cargando...";

        // 2. Las dos listas que se verán en la interfaz
        public ObservableCollection<ProcessItem> Processes { get; set; } = new();
        public ObservableCollection<StartupItem> StartupPrograms { get; set; } = new();

        public MainViewModel()
        {
            // Inicializamos los motores
            _hardwareService = new HardwareMonitorService();
            _processService = new ProcessService();
            _startupService = new StartupService();

            // Carga Estática: Leemos el registro de Windows UNA SOLA VEZ al arrancar
            LoadStartupPrograms();

            // Carga Dinámica: Bucle cada 2 segundos solo para lo que cambia en tiempo real
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) => UpdateLiveTelemetry();
            timer.Start();

            UpdateLiveTelemetry(); // Hacemos la primera lectura manual para no esperar 2 segundos
        }

        /// <summary>
        /// Lee el registro de Windows y llena la lista de programas de inicio.
        /// </summary>
        private void LoadStartupPrograms()
        {
            var startupList = _startupService.GetStartupItems();
            StartupPrograms.Clear();
            foreach (var item in startupList)
            {
                StartupPrograms.Add(item);
            }
        }

        /// <summary>
        /// Lee los sensores y los procesos activos. Esto es lo que se repite en bucle.
        /// </summary>
        private void UpdateLiveTelemetry()
        {
            // Pedimos a la placa base que lea los sensores
            _hardwareService.UpdateHardware();

            // Obtenemos y formateamos la CPU
            float load = _hardwareService.GetCpuLoad();
            CpuDisplay = $"{Math.Round(load, 1)} %";

            // Recargamos la lista de procesos activos
            var list = _processService.GetActiveProcesses();
            Processes.Clear();
            foreach (var item in list)
            {
                Processes.Add(item);
            }
        }

        [RelayCommand]
        public void TerminateProcess(ProcessItem process)
        {
            if (process == null) return;

            var result = MessageBox.Show(
                $"¿Seguro que quieres cerrar {process.Name}?\nSe perderán los datos no guardados.",
                "Confirmar acción",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool ok = _processService.KillProcess(process.Id);

                if (ok)
                {
                    // Si logramos matarlo, actualizamos la tabla al instante sin esperar al temporizador
                    UpdateLiveTelemetry();
                }
                else
                {
                    MessageBox.Show("No se pudo cerrar. Puede que no tengas permisos o el proceso ya haya terminado.");
                }
            }
        }

        [RelayCommand]
        public void ToggleStartup(StartupItem item)
        {
            if (item == null) return;

            string accion = item.IsEnabled ? "desactivar" : "activar";

            var result = MessageBox.Show(
                $"¿Seguro que quieres {accion} el inicio de {item.Name}?",
                "Modificar Arranque",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                bool ok = _startupService.ToggleStartupProgram(item);

                if (ok)
                {
                    // Recargamos la lista estática para reflejar los cambios
                    LoadStartupPrograms();
                }
                else
                {
                    MessageBox.Show("No se pudo modificar. Es posible que requieras ejecutar CoreCare como Administrador.");
                }
            }
        }

        // NUEVO COMANDO: Abrir ubicación del archivo
        [RelayCommand]
        public void OpenLocation(StartupItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Path)) return;

            try
            {
                string rutaLimpia = item.Path.Replace("\"", "");

                // Las mismas dos líneas mágicas que cortan la ruta
                if (rutaLimpia.Contains(" -")) rutaLimpia = rutaLimpia.Substring(0, rutaLimpia.IndexOf(" -"));
                if (rutaLimpia.Contains(" /")) rutaLimpia = rutaLimpia.Substring(0, rutaLimpia.IndexOf(" /"));

                rutaLimpia = rutaLimpia.Trim();
                rutaLimpia = Environment.ExpandEnvironmentVariables(rutaLimpia);

                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{rutaLimpia}\"");
            }
            catch
            {
                MessageBox.Show("No se pudo abrir la ubicación.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}