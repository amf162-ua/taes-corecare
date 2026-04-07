using CoreCare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
namespace CoreCare.Data
{
    // por ahora solo contiene la tabla donde se guarda los resultados del benchark y users
    public class CoreCareDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RegistroBenchmark> RegistrosBenchmark { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={ResolveDatabasePath()}");

        private static string ResolveDatabasePath()
        {
            var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            DirectoryInfo? current = baseDirectory;
            while (current != null)
            {
                string csprojPath = Path.Combine(current.FullName, "CoreCare.csproj");
                if (File.Exists(csprojPath))
                {
                    return Path.Combine(current.FullName, "corecare.db");
                }

                current = current.Parent;
            }

            // Fallback to project-relative path if project file was not found.
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "corecare.db"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.Plan)
                .HasConversion<string>();
        }

    }
}
