using System.Configuration;
using System.Data;
using System.Windows;
using CoreCare.Data;
using CoreCare.Services;

namespace CoreCare
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Comprueba si la base de datos existe y la crea si no existe,
        // así todos obtienemos la BD automáticamente
        // la primera vez que ejecutamos el proyecto. 
        // por ahora la db solo contiene la tabla de registros de resultados del benchmark y users
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new CoreCare.Data.CoreCareDbContext())
            {
                db.Database.EnsureCreated();
            }

            // Temporalmente forzamos a que arranque SIEMPRE la UI
            // bool runTerminalMenu = !e.Args.Contains("--ui", StringComparer.OrdinalIgnoreCase);
            bool runTerminalMenu = false; // <-- CAMBIO AQUÍ

            if (runTerminalMenu)
            {
                CoreCare.Services.TerminalBenchmarkMenuService.RunInteractiveMenu();
                Shutdown();
                return;
            }
        }

    }
    public partial class App : Application
    {
        public static bool IsUserLoggedIn { get; set; } = false;
        public static string CurrentUsername { get; set; } = "Invitado";

        // 👇 NUEVO: Estado inicial NO PREMIUM
        public static bool IsPremium { get; set; } = false;
    }
}
