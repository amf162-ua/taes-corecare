using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace CoreCare.Services
{
    /// <summary>
    /// Servicio centralizado de logging usando Serilog
    /// Proporciona métodos para registrar eventos, errores y información de la aplicación
    /// </summary>
    public static class LoggingService
    {
        private static Logger? _logger;
        private static readonly object _lockObject = new();

        /// <summary>
        /// Inicializa el logger de Serilog con la configuración por defecto
        /// </summary>
        public static void Initialize()
        {
            lock (_lockObject)
            {
                if (_logger != null)
                    return;

                try
                {
                    var logsDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
                    Directory.CreateDirectory(logsDirectory);

                    _logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.File(
                            path: Path.Combine(logsDirectory, "CoreCare-.txt"),
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                            retainedFileCountLimit: 30) // Mantener logs de los últimos 30 días
                        .CreateLogger();

                    Information("=== CoreCare Logging Service Initialized ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inicializando logging: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Registra un evento de información
        /// </summary>
        public static void Information(string message, params object[] args)
        {
            EnsureInitialized();
            _logger?.Information(message, args);
        }

        /// <summary>
        /// Registra una advertencia
        /// </summary>
        public static void Warning(string message, params object[] args)
        {
            EnsureInitialized();
            _logger?.Warning(message, args);
        }

        /// <summary>
        /// Registra un error
        /// </summary>
        public static void Error(string message, params object[] args)
        {
            EnsureInitialized();
            _logger?.Error(message, args);
        }

        /// <summary>
        /// Registra un error con excepción
        /// </summary>
        public static void Error(Exception exception, string message, params object[] args)
        {
            EnsureInitialized();
            _logger?.Error(exception, message, args);
        }

        /// <summary>
        /// Registra información de debug
        /// </summary>
        public static void Debug(string message, params object[] args)
        {
            EnsureInitialized();
            _logger?.Debug(message, args);
        }

        /// <summary>
        /// Registra un error fatal
        /// </summary>
        public static void Fatal(Exception exception, string message, params object[] args)
        {
            EnsureInitialized();
            _logger?.Fatal(exception, message, args);
        }

        private static void EnsureInitialized()
        {
            if (_logger == null)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Cierra y limpia el logger
        /// </summary>
        public static void Close()
        {
            lock (_lockObject)
            {
                _logger?.Dispose();
                _logger = null;
            }
        }

        /// <summary>
        /// Obtiene la instancia del logger de Serilog
        /// </summary>
        public static Logger? GetLogger() => _logger;
    }
}
