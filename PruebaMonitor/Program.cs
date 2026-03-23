using System;
using System.Threading;
using CoreCare.Services;

class Program
{
    static void Main()
    {
        Console.Title = "CoreCare Telemetry - Terminal de Prueba";

        using (var monitor = new HardwareMonitorService())
        {
            while (true)
            {
                //Actualizamos los sensores primero
                monitor.UpdateHardware();

                // Recolectamos los datos dinámicos
                var cpuTempData = monitor.GetCpuTemperature();
                float cpuLoad = monitor.GetCpuLoad();
                float cpuClock = monitor.GetCpuClockGHz();
                var osStats = monitor.GetSystemProcessesAndThreads();
                string upTime = monitor.GetUpTime();

                float ramUsed = monitor.GetRamUsageGb();
                float ramAvailable = monitor.GetRamAvailableGb();

                float gpuTemp = monitor.GetGpuTemperature();
                float gpuLoad = monitor.GetGpuLoad();

                Console.Clear();

                // --- SECCIÓN ESTÁTICA (Ficha Técnica) ---
                Console.WriteLine("=========================================================");
                Console.WriteLine("        FICHA TÉCNICA DEL PROCESADOR (ESTÁTICO)         ");
                Console.WriteLine("=========================================================");
                Console.WriteLine($" Sockets: {monitor.Sockets,-5} | Núcleos: {monitor.Cores,-5} | Hilos: {monitor.LogicalProcessors}");
                Console.WriteLine($" Caché L1: {monitor.CacheL1,-7} | L2: {monitor.CacheL2,-7} | L3: {monitor.CacheL3}");
                Console.WriteLine("=========================================================");

                // --- SECCIÓN DINÁMICA (Tiempo Real) ---
                Console.WriteLine("\n--- TELEMETRÍA EN TIEMPO REAL ---");
                Console.WriteLine($" [SISTEMA]");
                Console.WriteLine($" Tiempo de actividad: {upTime}");
                Console.WriteLine($" Procesos:            {osStats.Processes}");
                Console.WriteLine($" Subprocesos:         {osStats.Threads}");

                Console.WriteLine($"\n [CPU]");
                Console.WriteLine($" Carga:               {cpuLoad:F1} %");
                Console.WriteLine($" Frecuencia:          {cpuClock:F2} GHz");
                Console.WriteLine($" Temperatura:         {cpuTempData.Value:F1} ºC");

                Console.WriteLine($"\n [MEMORIA RAM]");
                Console.WriteLine($" En uso:              {ramUsed:F2} GB");
                Console.WriteLine($" Disponible:          {ramAvailable:F2} GB");

                Console.WriteLine($"\n [GPU]");
                Console.WriteLine($" Carga:               {gpuLoad:F1} %");
                Console.WriteLine($" Temperatura:         {gpuTemp:F1} ºC");

                Console.WriteLine("\n---------------------------------------------------------");
                Console.WriteLine(" Presiona Ctrl+C para cerrar la monitorización...");

                Thread.Sleep(1000);
            }
        }
    }
}