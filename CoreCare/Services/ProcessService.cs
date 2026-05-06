using CoreCare.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CoreCare.Services
{
    public class ProcessService
    {
        public List<ProcessItem> GetActiveProcesses()
        {
            var list = new List<ProcessItem>();
            var all = Process.GetProcesses();

            foreach (var p in all)
            {
                try
                {
                    // Filtro: Solo apps que el usuario ve (con ventana)
                    if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        list.Add(new ProcessItem
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            Description = p.MainModule?.FileVersionInfo.FileDescription ?? p.ProcessName,
                            RamUsageMB = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2),
                            IsCritical = p.ProcessName.Equals("CoreCare", StringComparison.OrdinalIgnoreCase) ||
                                         p.ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
                catch { /* Ignoramos procesos protegidos del sistema */ }
            }
            return list.OrderByDescending(x => x.RamUsageMB).ToList();
        }

        // ---> ESTA ES LA FUNCIÓN QUE TE FALTABA <---
        public bool KillProcess(int processId)
        {
            try
            {
                var processToKill = Process.GetProcessById(processId);
                processToKill.Kill();
                processToKill.WaitForExit(2000); // Da un margen de 2 segundos para que se cierre
                return true;
            }
            catch
            {
                // Falla si el proceso ya se cerró por su cuenta o requiere permisos de Administrador
                return false;
            }
        }
    }
}