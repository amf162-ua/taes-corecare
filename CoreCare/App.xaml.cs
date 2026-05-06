using System.Configuration;
using System.Data;
using System.Windows;
using CoreCare.Data;
using CoreCare.Orchestrators;
using CoreCare.Services;

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
    }
}