using System;
using System.Linq;
using LibreHardwareMonitor.Hardware; // Si esto sale en rojo, es que falta el NuGet

namespace CoreCare.Services
{
    public class HardwareMonitorService
    {
        private readonly Computer _computer;

        public HardwareMonitorService()
        {
            // Configuramos qué queremos monitorizar
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
            };

            _computer.Open();
        }

        public string GetCpuLoad()
        {
            try
            {
                // Buscamos el primer componente que sea CPU
                var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);

                if (cpu != null)
                {
                    cpu.Update(); // Refrescar datos del sensor

                    // Buscamos el sensor de carga (Load) total
                    var sensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total");

                    if (sensor != null)
                        return $"{sensor.Value:F1}%";
                }
                return "0.0%";
            }
            catch (Exception)
            {
                return "Error";
            }
        }

        public string GetRamUsage()
        {
            var ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
            if (ram != null)
            {
                ram.Update();
                var sensor = ram.Sensors.FirstOrDefault(s => s.Name == "Memory Used");
                return sensor != null ? $"{sensor.Value:F1} GB" : "N/A";
            }
            return "N/A";
        }
    }
}