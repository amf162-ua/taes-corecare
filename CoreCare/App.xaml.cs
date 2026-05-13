using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using CoreCare.Data;
using CoreCare.Orchestrators;
using CoreCare.Services;
using CoreCare.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using BCrypt.Net;

namespace CoreCare
{
    public partial class App : Application
    {
        // Estado global de sesión
        public static bool IsUserLoggedIn { get; set; } = false;
        public static string CurrentUsername { get; set; } = "Invitado";
        public static bool IsPremium { get; set; } = false;

        // Instancias únicas de los servicios
        public static HardwareMonitorService Monitor { get; private set; }
        public static ProcessService Processes { get; private set; }
        public static BenchmarkOrchestrator Orchestrator { get; private set; }

        // Contenedor de inyección de dependencias
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            // CAZADOR GLOBAL: Atrapa cualquier error inesperado de la interfaz y evita que se cierre en silencio
            this.DispatcherUnhandledException += (sender, e) =>
            {
                LoggingService.Error(e.Exception, "Error no manejado en la interfaz de usuario");
                MessageBox.Show($"Error crítico durante la ejecución:\n\n{e.Exception.Message}\n\n¿Falta alguna referencia?",
                                "Fallo Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // 0. Inicializar logging primero
                LoggingService.Initialize();
                LoggingService.Information("Aplicación iniciando...");

                QuestPDF.Settings.License = LicenseType.Community;

                // 0.5. Configurar Inyección de Dependencias
                LoggingService.Information("Configurando contenedor de inyección de dependencias...");
                Services = ConfigureDependencyInjection();
                LoggingService.Information("Contenedor DI configurado correctamente");

                // 1. Inicializamos los sensores globales
                LoggingService.Information("Inicializando servicios de hardware...");
                Monitor = new HardwareMonitorService();
                Processes = new ProcessService();
                Orchestrator = new BenchmarkOrchestrator(Monitor, new Models.StressWorker());
                LoggingService.Information("Servicios de hardware inicializados correctamente");

                // 2. Base de Datos
                LoggingService.Information("Inicializando base de datos...");
                using var db = new CoreCareDbContext();
                db.Database.EnsureCreated();
                SeedDefaultUsers(db);
                LoggingService.Information("Base de datos inicializada correctamente");

                // ¡AQUÍ ESTÁ LA MAGIA! Obligamos a WPF a pintar la ventana en pantalla.
                // (Nota: Si tu app empieza con una ventana de Login, cambia MainWindow por LoginWindow)
                LoggingService.Information("Mostrando ventana principal...");
                var mainWindow = new MainWindow();
                mainWindow.Show();

                LoggingService.Information("Aplicación iniciada correctamente");
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                LoggingService.Fatal(ex, "Error crítico al iniciar la aplicación");
                MessageBox.Show($"La aplicación no pudo arrancar:\n\n{ex.Message}", "Fallo", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
            finally
            {
                // Asegurarse de que el logging se cierra al salir
                if (this.ShutdownMode == ShutdownMode.OnExplicitShutdown || this.ShutdownMode == ShutdownMode.OnLastWindowClose)
                {
                    this.Exit += (s, e) => LoggingService.Close();
                }
            }
        }

        private static void SeedDefaultUsers(CoreCareDbContext db)
        {
            try
            {
                var admin = db.Users.FirstOrDefault(u => u.username == "admin" || u.email == "admin@admin.com" || u.email == "admin@corecare.local");
                if (admin == null)
                {
                    // Hash la contraseña con BCrypt
                    string adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin", workFactor: 12);

                    db.Users.Add(new User
                    {
                        name = "Administrador",
                        username = "admin",
                        email = "admin@admin.com",
                        password = adminPasswordHash, // Usar hash en lugar de texto plano
                        Role = UserRole.Administrador,
                        createdAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    db.SaveChanges();
                    LoggingService.Information("Usuario admin insertado por seeder (contraseña: admin)");
                }
                else
                {
                    LoggingService.Information("Usuario admin ya existe; seeder saltado para admin");
                }

                var client = db.Users.FirstOrDefault(u => u.username == "client" || u.email == "client@corecare.local");
                if (client == null)
                {
                    // Hash la contraseña con BCrypt
                    string clientPasswordHash = BCrypt.Net.BCrypt.HashPassword("client123", workFactor: 12);

                    db.Users.Add(new User
                    {
                        name = "Cliente Demo",
                        username = "client",
                        email = "client@corecare.local",
                        password = clientPasswordHash, // Usar hash en lugar de texto plano
                        Role = UserRole.Cliente,
                        createdAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    db.SaveChanges();
                    LoggingService.Information("Usuario demo cliente insertado por seeder (contraseña: client123)");
                }
                else
                {
                    LoggingService.Information("Usuario demo cliente ya existe; seeder saltado para client");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error en seeder de usuarios");
            }
        }

        /// <summary>
        /// Configura el contenedor de inyección de dependencias
        /// </summary>
        private static IServiceProvider ConfigureDependencyInjection()
        {
            try
            {
                var services = new ServiceCollection();

                // Registrar servicios
                // Nota: LoggingService y ValidationService son servicios estáticos singleton,
                // no se registran en el contenedor DI
                services.AddScoped<AuthService>();
                services.AddScoped<HardwareMonitorService>();
                services.AddScoped<ProcessService>();
                services.AddScoped<StartupService>();
                services.AddScoped<GeminiAIService>();
                services.AddScoped<SystemSpecsService>();
                services.AddScoped<HardwareUpgradeAdvisorService>();
                services.AddScoped<BenchmarkOrchestrator>();

                // Registrar DbContext
                services.AddScoped<CoreCareDbContext>();

                LoggingService.Information("Servicios registrados en el contenedor DI");
                return services.BuildServiceProvider();
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex, "Error configurando inyección de dependencias");
                throw;
            }
        }
    }
}
