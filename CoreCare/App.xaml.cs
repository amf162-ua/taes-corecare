using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CoreCare.Data;
using CoreCare.Orchestrators;
using CoreCare.Services;
using CoreCare.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreCare
{
    public partial class App : Application
    {
        // Estado global de sesión
        public static bool IsUserLoggedIn { get; set; } = false;
        public static string CurrentUsername { get; set; } = "Invitado";
        public static bool IsPremium { get; set; } = false;
        // Instancias únicas de los servicios para evitar conflictos de sensores
        public static HardwareMonitorService Monitor { get; } = new();
        public static ProcessService Processes { get; } = new ProcessService();
        public static BenchmarkOrchestrator Orchestrator { get; } = new(Monitor, new Models.StressWorker());

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var db = new CoreCareDbContext();
            db.Database.EnsureCreated();
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

    }
}
