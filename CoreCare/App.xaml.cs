using System.Configuration;
using System.Data;
using System.Windows;
using CoreCare.Data;

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

            // Aceptar licencia comunitaria y gratuita de QuestPDF globalmente en toda la aplicación
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            // Asegurarse de que la base de datos se crea al iniciar la aplicación
            using (var db = new CoreCareDbContext())
            {
                db.Database.EnsureCreated();
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
