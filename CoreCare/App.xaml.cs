using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CoreCare.Data;
using CoreCare.Models;
using Microsoft.EntityFrameworkCore;

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

            try
            {
                LogStartup("OnStartup entered");

                using (var db = new CoreCareDbContext())
                {
                    LogStartup($"Database file: {db.Database.GetDbConnection().DataSource}");
                    db.Database.EnsureCreated();
                    LogStartup("EnsureCreated completed");

                    EnsureUsersRoleColumn(db);
                    LogTableInfo(db);

                    // Ensure admin user exists (non-destructive). Also ensure a demo client exists if missing.
                    SeedDefaultUsers(db);

                    var users = db.Users
                        .Select(user => $"{user.username}:{user.Role}:{user.IsActive}")
                        .ToList();
                    LogStartup($"Users after seed: {string.Join(" | ", users)}");
                }

                var loginWindow = new LoginWindow();
                MainWindow = loginWindow;
                loginWindow.Show();
                LogStartup("Login window shown");
            }
            catch (Exception ex)
            {
                LogStartup($"Startup exception: {ex}");
                MessageBox.Show(ex.ToString(), "CoreCare startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private static void LogTableInfo(CoreCareDbContext db)
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info('Users');";

            using var reader = command.ExecuteReader();
            var columns = new List<string>();
            while (reader.Read())
            {
                columns.Add($"{reader.GetString(1)}:{reader.GetString(2)}");
            }

            LogStartup($"Users columns: {string.Join(", ", columns)}");
        }

        private static void EnsureUsersRoleColumn(CoreCareDbContext db)
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            var hasRoleColumn = false;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Users');";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(1), "Role", StringComparison.OrdinalIgnoreCase))
                    {
                        hasRoleColumn = true;
                        break;
                    }
                }
            }

            if (hasRoleColumn)
            {
                LogStartup("Role column already exists");
                return;
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "ALTER TABLE Users ADD COLUMN Role TEXT NOT NULL DEFAULT 'Cliente';";
                command.ExecuteNonQuery();
            }

            LogStartup("Role column added with direct SQL bootstrap");
        }

        private static void LogStartup(string message)
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "corecare-startup.log");
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            Debug.WriteLine(line);
            File.AppendAllText(logPath, line + Environment.NewLine);
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
