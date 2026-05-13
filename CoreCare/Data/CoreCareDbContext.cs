using CoreCare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
namespace CoreCare.Data
{
    // Persiste usuarios, resultados de benchmark y las lecturas detalladas de sensores
    public class CoreCareDbContext : DbContext
    {
        public CoreCareDbContext()
        {
        }

        public CoreCareDbContext(DbContextOptions<CoreCareDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RegistroBenchmark> RegistrosBenchmark { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite($"Data Source={ResolveDatabasePath()}");
            }
        }

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
            // Use a tolerant converter: accept old English values in existing DB rows
            var roleConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<UserRole, string>(
                v => v == UserRole.Administrador ? "Administrador" : "Cliente",
                v => (v == "Client" || v == "client" || v == "cliente") ? UserRole.Cliente :
                     (v == "Management" || v == "management" || v == "Admin" || v == "admin" || v == "Administrador" || v == "administrador") ? UserRole.Administrador :
                     UserRole.Cliente
            );

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(roleConverter)
                .HasDefaultValue(UserRole.Cliente);

            modelBuilder.Entity<User>()
                .Property(u => u.Plan)
                .HasConversion<string>();

            // Chat relationships
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
