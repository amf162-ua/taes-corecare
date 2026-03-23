using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace CoreCare.Services
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct TelemetryData
    {
        public float Value;
        public long Timestamp;
    }

    public class HardwareMonitorService : IDisposable
    {
        private readonly Computer _computer;
        private readonly int _windowLimit = 5;

        private readonly float[] _cpuTempWindow;
        private int _tempWindowIndex = 0;
        private int _tempReadingsCount = 0;

        private readonly float[] _cpuLoadWindow;
        private int _loadWindowIndex = 0;
        private int _loadReadingsCount = 0;

        public int Sockets { get; private set; }
        public int Cores { get; private set; }
        public int LogicalProcessors { get; private set; }
        public string CacheL1 { get; private set; } = "N/A";
        public string CacheL2 { get; private set; } = "N/A";
        public string CacheL3 { get; private set; } = "N/A";

        public HardwareMonitorService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true
            };
            _computer.Open();

            _cpuTempWindow = new float[_windowLimit];
            _cpuLoadWindow = new float[_windowLimit];

            LoadProcessorStaticData();
        }

        private void LoadProcessorStaticData()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    Sockets++;
                    Cores = Convert.ToInt32(obj["NumberOfCores"]);
                    LogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);

                    CacheL1 = $"{Cores * 64} KB";

                    CacheL2 = $"{obj["L2CacheSize"]} KB";

                    uint l3Kb = Convert.ToUInt32(obj["L3CacheSize"]);
                    CacheL3 = l3Kb >= 1024 ? $"{(l3Kb / 1024.0):F1} MB" : $"{l3Kb} KB";
                }
            }
            catch { }
        }

        public float GetCpuClockGHz()
        {
            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);

            var coreClocks = cpu?.Sensors
                .Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("Core"))
                .ToList();

            float currentMhz = 0f;

            if (coreClocks != null && coreClocks.Any())
            {
                currentMhz = (float)coreClocks.Average(c => c.Value);
            }

            if (currentMhz == 0f)
            {
                currentMhz = GetWmiCpuClock();
            }

            return currentMhz / 1000f;
        }

        public void UpdateHardware()
        {
            foreach (var hw in _computer.Hardware) hw.Update();
        }

        public string GetUpTime()
        {
            var tickCount = Environment.TickCount64;
            var uptime = TimeSpan.FromMilliseconds(tickCount);

            return $"{uptime.Days}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }

        public TelemetryData GetCpuTemperature()
        {
            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            var sensor = cpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Value > 0);
            float rawTemp = sensor?.Value ?? GetWmiCpuTemperature();

            _cpuTempWindow[_tempWindowIndex] = rawTemp;
            _tempWindowIndex = (_tempWindowIndex + 1) % _windowLimit;
            if (_tempReadingsCount < _windowLimit) _tempReadingsCount++;

            return new TelemetryData
            {
                Value = _cpuTempWindow.Take(_tempReadingsCount).Average(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

       
        public float GetCpuLoad()
        {
            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            var sensor = cpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Value > 0);
            float rawLoad = sensor?.Value ?? GetWmiCpuLoad();

            _cpuLoadWindow[_loadWindowIndex] = rawLoad;
            _loadWindowIndex = (_loadWindowIndex + 1) % _windowLimit;
            if (_loadReadingsCount < _windowLimit) _loadReadingsCount++;

            return _cpuLoadWindow.Take(_loadReadingsCount).Average();
        }

        private float GetWmiCpuTemperature() { /* ... */ try { using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"); foreach (var obj in searcher.Get()) return (Convert.ToSingle(obj["CurrentTemperature"]) / 10f) - 273.15f; } catch { } return 0f; }
        private float GetWmiCpuClock() { try { using var searcher = new ManagementObjectSearcher("select CurrentClockSpeed from Win32_Processor"); foreach (var item in searcher.Get()) return Convert.ToSingle(item["CurrentClockSpeed"]); } catch { } return 0f; }
        private float GetWmiCpuLoad() { try { using var searcher = new ManagementObjectSearcher("select LoadPercentage from Win32_Processor"); foreach (var item in searcher.Get()) return Convert.ToSingle(item["LoadPercentage"]); } catch { } return 0f; }

        public (int Processes, int Threads) GetSystemProcessesAndThreads()
        {
            var processes = Process.GetProcesses();
            int threadCount = 0;
            foreach (var p in processes) { try { threadCount += p.Threads.Count; } catch { } }
            return (processes.Length, threadCount);
        }

        public float GetRamUsageGb() { var ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory); return ram?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Memory Used")?.Value ?? 0f; }
        public float GetRamAvailableGb() { var ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory); return ram?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Memory Available")?.Value ?? 0f; }
        public float GetGpuLoad() { var gpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd); return gpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Value > 0)?.Value ?? 0f; }
        public float GetGpuTemperature() { var gpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd); return gpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Value > 0)?.Value ?? 0f; }

        public void Dispose() => _computer.Close();
    }
}