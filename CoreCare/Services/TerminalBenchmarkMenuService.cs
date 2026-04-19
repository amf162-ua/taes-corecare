using System;
using System.Linq;
using System.Runtime.InteropServices;
using CoreCare.Models;
using CoreCare.Orchestrators;
using CoreCare.Data;

namespace CoreCare.Services
{
    public static class TerminalBenchmarkMenuService
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        public static void RunInteractiveMenu()
        {
            EnsureConsole();

            Console.Title = "CoreCare - Menu de benchmark";

            using var monitor = new HardwareMonitorService();
            var stressWorker = new Models.StressWorker();
            var orchestrator = new BenchmarkOrchestrator(monitor, stressWorker);

            while (true)
            {
                Console.Clear();
                PrintMenu();

                string? input = Console.ReadLine()?.Trim();

                if (input == "0")
                {
                    break;
                }

                if (!TryMapSelection(input, out var options))
                {
                    Console.WriteLine("\nOpcion invalida. Pulsa una tecla para continuar...");
                    Console.ReadKey(intercept: true);
                    continue;
                }

                const int durationSeconds = 8;

                Console.WriteLine("\nIniciando benchmark...");
                Console.WriteLine($"Duracion: {durationSeconds} segundos");

                var benchmarkTask = Task.Run(() => orchestrator.RunBenchmarkAsync(options, durationSeconds));

                if (!benchmarkTask.Wait(TimeSpan.FromSeconds(durationSeconds + 20)))
                {
                    stressWorker.Stop();
                    Console.WriteLine("\nEl benchmark no finalizo a tiempo. Revisa sensores GPU/CPU y vuelve a intentar.");
                    Console.WriteLine("Pulsa una tecla para volver al menu...");
                    Console.ReadKey(intercept: true);
                    continue;
                }

                RegistroBenchmark registro;
                try
                {
                    registro = benchmarkTask.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError durante benchmark: {ex.GetBaseException().Message}");
                    Console.WriteLine("Pulsa una tecla para volver al menu...");
                    Console.ReadKey(intercept: true);
                    continue;
                }

                try
                {
                    using var db = new CoreCareDbContext();
                    registro.UserId = GetOrCreateSystemUserId(db);
                    db.RegistrosBenchmark.Add(registro);
                    db.SaveChanges();
                    Console.WriteLine($"\nResultado guardado en la base de datos con Id {registro.Id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nNo se pudo guardar el resultado: {ex.GetBaseException().Message}");
                }

                PrintSummary(registro, options);

                Console.WriteLine("\nPulsa una tecla para volver al menu...");
                Console.ReadKey(intercept: true);
            }
        }

        private static void EnsureConsole()
        {
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                AllocConsole();
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("============================================");
            Console.WriteLine(" CoreCare - Prueba de benchmark por consola ");
            Console.WriteLine("============================================");
            Console.WriteLine("1) Monitor CPU");
            Console.WriteLine("2) Monitor GPU");
            Console.WriteLine("3) Monitor RAM");
            Console.WriteLine("4) Monitor Disco");
            Console.WriteLine("5) Monitor Todo");
            Console.WriteLine("0) Salir");
            Console.Write("\nSelecciona una opcion: ");
        }

        private static bool TryMapSelection(string? input, out ScanOptions options)
        {
            options = ScanOptions.ScanCPUOnly();

            switch (input)
            {
                case "1":
                    options = ScanOptions.ScanCPUOnly();
                    return true;
                case "2":
                    options = ScanOptions.ScanGPUOnly();
                    return true;
                case "3":
                    options = ScanOptions.ScanRAMOnly();
                    return true;
                case "4":
                    options = ScanOptions.ScanDiskOnly();
                    return true;
                case "5":
                    options = ScanOptions.FullScan();
                    return true;
                default:
                    return false;
            }
        }

        private static void PrintSummary(RegistroBenchmark registro, ScanOptions options)
        {
            Console.WriteLine("\n---------------- RESULTADO ----------------");
            Console.WriteLine($"Timestamp: {registro.Timestamp:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Score:     {registro.Score:F1}/10");

            var unavailableReadings = registro.SensorReadings
                .Where(reading => reading.Name.Contains("NO DISPONIBLE", StringComparison.OrdinalIgnoreCase))
                .Select(reading => reading.Name)
                .Distinct()
                .ToList();

            if (options.ScanCPU)
            {
                Console.WriteLine("\n[CPU]");
                Console.WriteLine($"Carga media:      {registro.CpuLoad:F1} %");
                Console.WriteLine($"Temperatura media:{registro.CpuTemp:F1} C");
                Console.WriteLine($"Frecuencia media: {registro.CpuClock:F2} GHz");
            }

            if (options.ScanGPU)
            {
                Console.WriteLine("\n[GPU]");
                Console.WriteLine($"Carga media:      {registro.GpuLoad:F1} %");
                Console.WriteLine($"Temperatura media:{registro.GpuTemp:F1} C");
            }

            if (options.ScanRAM)
            {
                Console.WriteLine("\n[RAM]");
                Console.WriteLine($"Uso medio:        {registro.RamUsed:F2} GB");
                Console.WriteLine($"Carga media:      {registro.RamLoad:F1} %");
            }

            if (options.ScanDisk)
            {
                Console.WriteLine("\n[DISCO]");
                Console.WriteLine($"Carga media:      {registro.DiskLoad:F1} %");
                Console.WriteLine($"Lectura media:    {registro.DiskReadRate:F2} MB/s");
                Console.WriteLine($"Escritura media:  {registro.DiskWriteRate:F2} MB/s");
            }

            if (unavailableReadings.Any())
            {
                Console.WriteLine("\n[SENSORES NO DISPONIBLES]");
                foreach (var unavailable in unavailableReadings)
                {
                    Console.WriteLine($"- {unavailable}");
                }
            }

            Console.WriteLine("-------------------------------------------");
        }

        private static int GetOrCreateSystemUserId(CoreCareDbContext db)
        {
            var existingUser = db.Users.FirstOrDefault(user => user.IsActive);

            if (existingUser != null)
            {
                return existingUser.Id;
            }

            var systemUser = new User
            {
                name = "prueba",
                username = "prueba",
                email = "prueba@corecare.local",
                password = string.Empty,
                createdAt = DateTime.UtcNow,
                IsActive = true,
                Plan = TipoPlan.Basico
            };

            db.Users.Add(systemUser);
            db.SaveChanges();

            return systemUser.Id;
        }
    }
}