using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CoreCare.Models;

namespace CoreCare.Views.Components
{
    public partial class ProcessManager : UserControl
    {
        private ObservableCollection<SystemProcess> _processes;
        private DispatcherTimer _refreshTimer;

        public ProcessManager()
        {
            InitializeComponent();
            _processes = new ObservableCollection<SystemProcess>();
            ProcessList.ItemsSource = _processes;

            // Refresco automático cada 5 segundos
            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _refreshTimer.Tick += (s, e) => LoadProcesses();
            _refreshTimer.Start();

            LoadProcesses();
        }

        private void LoadProcesses()
        {
            // Obtener procesos reales del servicio[cite: 8]
            var activeProcesses = App.Processes.GetActiveProcesses();

            _processes.Clear();
            foreach (var p in activeProcesses)
            {
                _processes.Add(new SystemProcess
                {
                    Pid = p.Id,
                    Name = p.Name,
                    RamUsage = Math.Round(p.RamUsageMB / 1024.0, 2), // Convertimos a GB para el diseño[cite: 8, 13]
                    Status = p.RamUsageMB > 1500 ? "critical" : "running" // Crítico si supera 1.5GB[cite: 12]
                });
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            TxtActiveCount.Text = _processes.Count.ToString();
            TxtCriticalCount.Text = _processes.Count(p => p.Status == "critical").ToString();
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var process = btn?.DataContext as SystemProcess;

            if (process == null) return;

            // Intentar cerrar el proceso de forma real[cite: 8]
            if (App.Processes.KillProcess(process.Pid))
            {
                // Pequeña pausa visual antes de quitarlo de la lista[cite: 12]
                await System.Threading.Tasks.Task.Delay(500);
                _processes.Remove(process);
                UpdateStats();
            }
            else
            {
                MessageBox.Show("No se puede cerrar este proceso. Puede ser una aplicación del sistema protegida.");
            }
        }
    }
}