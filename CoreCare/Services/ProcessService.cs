using CoreCare.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
                            RamUsageMB = p.WorkingSet64 / 1024 / 1024
                        });
                    }
                }
                catch { /* Ignoramos procesos protegidos del sistema */ }
            }
            return list.OrderByDescending(x => x.RamUsageMB).ToList();
        }
    }
}
