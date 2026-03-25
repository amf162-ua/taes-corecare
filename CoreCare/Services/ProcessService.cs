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

            // Lista negra de nombres de procesos críticos del sistema
            string[] systemWhitelist = {
        "explorer", "idle", "system", "svchost", "wininit",
        "services", "lsass", "smss", "csrss", "winlogon"
    };

            foreach (var p in all)
            {
                try
                {
                    // 1. Saltamos si es un proceso crítico por nombre
                    if (systemWhitelist.Contains(p.ProcessName.ToLower())) continue;

                    // 2. Filtro: Solo procesos con ventana (apps de usuario)
                    // Esto descarta la mayoría de servicios en segundo plano peligrosos
                    if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        // 3. Comprobación adicional de seguridad: ruta del archivo
                        string? fileName = p.MainModule?.FileName?.ToLower();
                        if (fileName != null && fileName.Contains("c:\\windows\\system32")) continue;

                        list.Add(new ProcessItem
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            Description = p.MainModule?.FileVersionInfo.FileDescription ?? p.ProcessName,
                            RamUsageMB = p.WorkingSet64 / 1024 / 1024
                        });
                    }
                }
                catch { /* Procesos protegidos que no podemos ni leer se ignoran por seguridad */ }
            }
            return list.OrderByDescending(x => x.RamUsageMB).ToList();
        }

        public bool TerminateProcess(int processId)
        {
            try
            {
                var p = Process.GetProcessById(processId);

                // Intentar cierre elegante (como darle a la X de la ventana)
                p.CloseMainWindow();

                // Esperar un momento a que cierre solo
                if (!p.WaitForExit(2000))
                {
                    p.Kill(); // Si no hace caso en 2s, forzamos el cierre
                }
                return true;
            }
            catch (Exception)
            {
                return false; // No tenemos permisos o ya se cerró
            }
        }
    }
}
