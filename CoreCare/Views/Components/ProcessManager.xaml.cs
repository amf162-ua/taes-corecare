using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models;

namespace CoreCare.Views.Components
{
    public partial class ProcessManager : UserControl
    {
        // ObservableCollection actualiza la lista en pantalla automáticamente si añadimos/quitamos items
        private ObservableCollection<SystemProcess> _processes;

        public ProcessManager()
        {
            InitializeComponent();
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            _processes = new ObservableCollection<SystemProcess>
            {
                new SystemProcess { Id = "1", Name = "Chrome.exe", CpuUsage = 45, RamUsage = 2.5, Status = "running", Pid = 8472 },
                new SystemProcess { Id = "4", Name = "Background Svc", CpuUsage = 92, RamUsage = 4.2, Status = "critical", Pid = 1923 },
                new SystemProcess { Id = "5", Name = "System Process", CpuUsage = 78, RamUsage = 3.5, Status = "critical", Pid = 4412, IsProtected = true },
                new SystemProcess { Id = "6", Name = "Windows Update", CpuUsage = 35, RamUsage = 1.2, Status = "running", Pid = 6721 },
                new SystemProcess { Id = "7", Name = "Antivirus Scan", CpuUsage = 88, RamUsage = 2.9, Status = "critical", Pid = 2156, IsProtected = true }
            };

            ProcessList.ItemsSource = _processes;
            UpdateStats();
        }

        private void UpdateStats()
        {
            TxtActiveCount.Text = _processes.Count(p => p.Status == "running" || p.Status == "critical").ToString();
            TxtCriticalCount.Text = _processes.Count(p => p.Status == "critical").ToString();
            TxtStoppedCount.Text = _processes.Count(p => p.Status == "stopped").ToString();
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var process = btn?.DataContext as SystemProcess;

            if (process == null) return;

            // 1. Estado "Deteniendo..."
            process.Status = "stopping";

            // 2. Esperamos 2 segundos sin congelar la pantalla (Magia de Async/Await)
            await Task.Delay(2000);

            // 3. Evaluamos si se detiene o falla por protección
            if (process.IsProtected)
            {
                process.Status = "failed";
            }
            else
            {
                process.Status = "stopped";
                process.CpuUsage = 0;
                process.RamUsage = 0;
                UpdateStats();

                // 4. Opcional: Eliminar de la lista tras 1 segundo
                await Task.Delay(1000);
                _processes.Remove(process);
            }

            UpdateStats();
        }
    }
}