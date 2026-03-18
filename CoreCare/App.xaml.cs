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
        // por ahora la db solo contiene la tabla de registros de resultados del benchmark
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Asegurarse de que la base de datos se crea al iniciar la aplicación
            using (var db = new CoreCareDbContext())
            {
                db.Database.EnsureCreated();
            }
        }
    }

}
