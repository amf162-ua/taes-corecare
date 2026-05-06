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
using QuestPDF.Infrastructure;

namespace CoreCare
{
    public partial class App : Application
    {
        // Estado global de sesión
        public static bool IsUserLoggedIn { get; set; } = false;
        public static string CurrentUsername { get; set; } = "Invitado";
        public static bool IsPremium { get; set; } = false;

        // Instancias únicas de los servicios (Quitamos el '= new()' de aquí para controlarlo abajo)
        public static HardwareMonitorService Monitor { get; private set; }
        public static ProcessService Processes { get; private set; }
        public static BenchmarkOrchestrator Orchestrator { get; private set; }

        public App()
        {
            // CAZADOR GLOBAL: Atrapa cualquier error inesperado de la interfaz y evita que se cierre en silencio
            this.DispatcherUnhandledException += (sender, e) =>
            {
                MessageBox.Show($"Error crítico durante la ejecución:\n\n{e.Exception.Message}\n\n¿Falta alguna referencia?",
                                "Fallo Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                // 1. Inicializamos los sensores globales
                Monitor = new HardwareMonitorService();
                Processes = new ProcessService();
                Orchestrator = new BenchmarkOrchestrator(Monitor, new Models.StressWorker());

                // 2. Base de Datos
                using var db = new CoreCareDbContext();
                db.Database.EnsureCreated();
                SeedDefaultUsers(db);

                // ¡AQUÍ ESTÁ LA MAGIA! Obligamos a WPF a pintar la ventana en pantalla.
                // (Nota: Si tu app empieza con una ventana de Login, cambia MainWindow por LoginWindow)
                var mainWindow = new MainWindow();
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"La aplicación no pudo arrancar:\n\n{ex.Message}", "Fallo", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private static void SeedDefaultUsers(CoreCareDbContext db)
        {
            try
            {
                var admin = db.Users.FirstOrDefault(u => u.username == "admin" || u.email == "admin@admin.com" || u.email == "admin@corecare.local");
                if (admin == null)
                {
                    db.Users.Add(new User
                    {
                        name = "Administrador",
                        username = "admin",
                        email = "admin@admin.com",
                        password = "admin",
                        Role = UserRole.Administrador,
                        createdAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    db.SaveChanges();
                    LogStartup("Admin user inserted by seeder");
                }
                else
                {
                    LogStartup("Admin user already exists; seeder skipped for admin");
                }

                var client = db.Users.FirstOrDefault(u => u.username == "client" || u.email == "client@corecare.local");
                if (client == null)
                {
                    db.Users.Add(new User
                    {
                        name = "Cliente Demo",
                        username = "client",
                        email = "client@corecare.local",
                        password = "client123",
                        Role = UserRole.Cliente,
                        createdAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    db.SaveChanges();
                    LogStartup("Demo client inserted by seeder");
                }
                else
                {
                    LogStartup("Demo client already exists; seeder skipped for client");
                }
            }
            catch (Exception ex)
            {
                LogStartup($"Seeder failed: {ex.Message}");
            }
        }

        private static void LogStartup(string message)
        {
            Debug.WriteLine($"[CoreCare DB-Seeder] {message}");
        }
    }
}
