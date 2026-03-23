using System;
using System.Threading;
using CoreCare.Services;

class Program
{
    static void Main()
    {
        Console.WriteLine("Iniciando monitorización de componentes... (Presiona Ctrl+C para salir)");
        Console.WriteLine("-------------------------------------------------------------------------");

        using (var monitor = new HardwareMonitorService())
        {
            while (true)
            {
                monitor.UpdateHardware();

                var cpuTempData = monitor.GetCpuTemperature();
                float cpuLoad = monitor.GetCpuLoad();
                float gpuTemp = monitor.GetGpuTemperature();
                float gpuLoad = monitor.GetGpuLoad();
                float ramUsed = monitor.GetRamUsageGb();

                Console.Clear();
                Console.WriteLine($"--- TELEMETRÍA EN TIEMPO REAL ---");
                Console.WriteLine($"[Timestamp Alineado]: {cpuTempData.Timestamp}");
                Console.WriteLine($"CPU Carga:        {cpuLoad:F1} %");
                Console.WriteLine($"CPU Temp (Media): {cpuTempData.Value:F1} ºC (Suavizada por ventana)");
                Console.WriteLine($"GPU Carga:        {gpuLoad:F1} %");
                Console.WriteLine($"GPU Temp:         {gpuTemp:F1} ºC");
                Console.WriteLine($"RAM Usada:        {ramUsed:F2} GB");

                Thread.Sleep(1000);
            }
        }
    }
}