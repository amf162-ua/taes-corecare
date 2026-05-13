using Microsoft.Win32;
using CoreCare.Models;
using System.Collections.Generic;
using System;

namespace CoreCare.Services
{
    public class StartupService
    {
        private readonly string _runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private readonly string _wow64Key = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run";

        // Nuestra propia carpeta en el Registro de Windows
        private readonly string _coreCareDisabledKey = @"SOFTWARE\CoreCare\Disabled";

        public List<StartupItem> GetStartupItems()
        {
            try
            {
                var items = new List<StartupItem>();

                // 1. Leemos los ACTIVOS (Puerta normal)
                LeerClaveRegistro(Registry.CurrentUser, _runKey, items, true);
                LeerClaveRegistro(Registry.CurrentUser, _wow64Key, items, true);
                try { LeerClaveRegistro(Registry.LocalMachine, _runKey, items, true); }
                catch (Exception ex)
                {
                    LoggingService.Warning("No se pudo leer programas de inicio en HKLM. Detalle: {Message}", ex.Message);
                }

                // 2. Leemos los DESACTIVADOS (Nuestra zona de cuarentena)
                LeerClaveRegistro(Registry.CurrentUser, _coreCareDisabledKey, items, false);

                LoggingService.Information("Se obtuvieron {Count} programas de inicio", items.Count);
                return items;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error obteniendo programas de inicio");
                return new List<StartupItem>();
            }
        }

        // Función auxiliar para no repetir código leyendo el registro
        private void LeerClaveRegistro(RegistryKey baseKey, string path, List<StartupItem> list, bool isEnabled)
        {
            try
            {
                using (RegistryKey? key = baseKey.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            try
                            {
                                if (!list.Exists(i => i.Name == valueName))
                                {
                                    string ruta = key.GetValue(valueName)?.ToString() ?? "Desconocido";
                                    list.Add(new StartupItem
                                    {
                                        Name = valueName,
                                        Path = ruta,
                                        IsEnabled = isEnabled,
                                        Publisher = ExtraerFabricante(ruta) // Llenamos la nueva propiedad
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggingService.Debug("Error leyendo entrada de registro: {EntryName}. Detalle: {Message}", valueName, ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.Debug("Error leyendo clave de registro: {Path}. Detalle: {Message}", path, ex.Message);
            }
        }

        // Función "Interruptor" que mueve el programa de un sitio a otro
        public bool ToggleStartupProgram(StartupItem item)
        {
            try
            {
                if (item.IsEnabled)
                {
                    // Si estaba activo, lo DESACTIVAMOS (Lo movemos a CoreCare)
                    LoggingService.Information("Desactivando programa de inicio: {Name}", item.Name);
                    MoverRegistro(_runKey, _coreCareDisabledKey, item.Name, item.Path);
                    LoggingService.Information("Programa de inicio desactivado exitosamente: {Name}", item.Name);
                }
                else
                {
                    // Si estaba desactivado, lo ACTIVAMOS (Lo devolvemos a Run)
                    LoggingService.Information("Activando programa de inicio: {Name}", item.Name);
                    MoverRegistro(_coreCareDisabledKey, _runKey, item.Name, item.Path);
                    LoggingService.Information("Programa de inicio activado exitosamente: {Name}", item.Name);
                }
                return true;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                LoggingService.Warning("Acceso denegado al modificar programa de inicio: {Name} - se requieren permisos de administrador. Detalle: {Message}", item.Name, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error al cambiar estado del programa de inicio: {Name}", item.Name);
                return false;
            }
        }

        private void MoverRegistro(string origen, string destino, string nombre, string ruta)
        {
            try
            {
                // Borramos del origen
                using (RegistryKey? keyOrigen = Registry.CurrentUser.OpenSubKey(origen, true))
                {
                    keyOrigen?.DeleteValue(nombre, false);
                }

                // Escribimos en el destino (creando la carpeta CoreCare si no existe)
                using (RegistryKey keyDestino = Registry.CurrentUser.CreateSubKey(destino, true))
                {
                    keyDestino.SetValue(nombre, ruta);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error moviendo entrada de registro: {Name}", nombre);
                throw;
            }
        }

        // Extrae la empresa que creó el programa
        private string ExtraerFabricante(string rutaBruta)
        {
            try
            {
                // 1. Quitamos las comillas
                string rutaLimpia = rutaBruta.Replace("\"", "");

                // 2. Cortamos la ruta si hay comandos con "-" o con "/"
                if (rutaLimpia.Contains(" -"))
                    rutaLimpia = rutaLimpia.Substring(0, rutaLimpia.IndexOf(" -"));
                if (rutaLimpia.Contains(" /"))
                    rutaLimpia = rutaLimpia.Substring(0, rutaLimpia.IndexOf(" /"));

                rutaLimpia = rutaLimpia.Trim();

                // 3. (Extra de seguridad) Expandimos variables como %appdata%
                rutaLimpia = Environment.ExpandEnvironmentVariables(rutaLimpia);

                if (System.IO.File.Exists(rutaLimpia))
                {
                    var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(rutaLimpia);
                    return !string.IsNullOrWhiteSpace(info.CompanyName) ? info.CompanyName : "Desconocido";
                }
            }
            catch (Exception ex)
            {
                LoggingService.Debug("Error extrayendo fabricante de: {Path}. Detalle: {Message}", rutaBruta, ex.Message);
            }
            return "Desconocido";
        }
    }
}