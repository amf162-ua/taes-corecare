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
            try
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
                    catch (Exception ex)
                    {
                        LoggingService.Debug("Error obteniendo información del proceso {ProcessId}: {Message}", p.Id, ex.Message);
                        // Ignoramos procesos protegidos del sistema
                    }
                }
                return list.OrderByDescending(x => x.RamUsageMB).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error obteniendo procesos activos");
                return new List<ProcessItem>();
            }
        }

        // ---> ESTA ES LA FUNCIÓN QUE TE FALTABA <---
        public bool KillProcess(int processId)
        {
            try
            {
                var processToKill = Process.GetProcessById(processId);
                processToKill.Kill();
                processToKill.WaitForExit(2000); // Da un margen de 2 segundos para que se cierre
                LoggingService.Information("Proceso {ProcessName} (ID: {ProcessId}) terminado correctamente", processToKill.ProcessName, processId);
                return true;
            }
            catch (ArgumentException)
            {
                LoggingService.Warning("Intento de terminar proceso que no existe o ya está cerrado: {ProcessId}", processId);
                return false;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                LoggingService.Warning("Acceso denegado al intentar terminar proceso {ProcessId} - se requieren permisos de administrador. Detalle: {Message}", processId, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error inesperado al intentar terminar proceso {ProcessId}", processId);
                return false;
            }
        }
    }
}