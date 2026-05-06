using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using System.Threading;
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
        private readonly object _updateLock = new();
        private readonly int _windowLimit = 5;
        private const float MaxPlausibleDiskThroughputMbit = 200_000f;

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

        private const string UnavailableReason = "No sensor data available.";

        public HardwareMonitorService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true
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

        public bool TryGetCpuClockGHz(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            if (cpu is null)
            {
                reason = "CPU hardware not detected.";
                return false;
            }

            var coreClocks = cpu.Sensors
                .Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("Core"))
                .Where(s => s.Value.HasValue)
                .ToList();

            float currentMhz = 0f;

            if (coreClocks != null && coreClocks.Any())
            {
                currentMhz = coreClocks.Average(c => c.Value!.Value);
            }

            if (currentMhz == 0f)
            {
                var wmiClock = TryGetWmiCpuClock();
                if (wmiClock.HasValue)
                {
                    currentMhz = wmiClock.Value;
                }
            }

            if (currentMhz <= 0f)
            {
                reason = "CPU clock sensor and WMI fallback did not return data.";
                return false;
            }

            value = currentMhz / 1000f;
            return true;
        }

        public float GetCpuClockGHz()
            => TryGetCpuClockGHz(out var value, out _) ? value : 0f;

        public void UpdateHardware()
        {
            lock (_updateLock)
            {
                foreach (var hw in _computer.Hardware)
                {
                    if (hw is null)
                    {
                        continue;
                    }

                    try
                    {
                        hw.Update();
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var subHw in hw.SubHardware)
                    {
                        if (subHw is null)
                        {
                            continue;
                        }

                        try
                        {
                            subHw.Update();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public string GetUpTime()
        {
            var tickCount = Environment.TickCount64;
            var uptime = TimeSpan.FromMilliseconds(tickCount);

            return $"{uptime.Days}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }

        public bool TryGetCpuTemperature(out TelemetryData value, out string reason)
        {
            value = default;
            reason = string.Empty;

            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            cpu?.Update();

            float? rawTemp = TrySelectCpuTemperature(cpu);

            if (!rawTemp.HasValue)
            {
                rawTemp = TryGetWmiCpuTemperature();
            }

            if (!rawTemp.HasValue)
            {
                reason = "CPU temperature sensor and WMI fallback did not return data.";
                return false;
            }

            _cpuTempWindow[_tempWindowIndex] = rawTemp.Value;
            _tempWindowIndex = (_tempWindowIndex + 1) % _windowLimit;
            if (_tempReadingsCount < _windowLimit) _tempReadingsCount++;

            float avgTemp = _cpuTempWindow.Take(_tempReadingsCount).Average();

            value = new TelemetryData
            {
                Value = avgTemp,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            return true;
        }

        public TelemetryData GetCpuTemperature()
            => TryGetCpuTemperature(out var value, out _) ? value : default;

       
        public bool TryGetCpuLoad(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            float? rawLoad = cpu?.Sensors
                .FirstOrDefault(s => s.SensorType == SensorType.Load && s.Value.HasValue)
                ?.Value;

            if (!rawLoad.HasValue)
            {
                rawLoad = TryGetWmiCpuLoad();
            }

            if (!rawLoad.HasValue)
            {
                reason = "CPU load sensor and WMI fallback did not return data.";
                return false;
            }

            _cpuLoadWindow[_loadWindowIndex] = rawLoad.Value;
            _loadWindowIndex = (_loadWindowIndex + 1) % _windowLimit;
            if (_loadReadingsCount < _windowLimit) _loadReadingsCount++;

            value = _cpuLoadWindow.Take(_loadReadingsCount).Average();
            return true;
        }

        public float GetCpuLoad()
            => TryGetCpuLoad(out var value, out _) ? value : 0f;

        private float? TrySelectCpuTemperature(IHardware? cpu)
        {
            var sensors = new List<ISensor>();

            if (cpu is not null)
            {
                sensors.AddRange(GetTemperatureSensors(cpu));
            }

            sensors.AddRange(_computer.Hardware
                .Where(h => h.HardwareType == HardwareType.Motherboard)
                .SelectMany(GetTemperatureSensors)
                .Where(s => IsLikelyCpuTemperatureSensor(s.Name)));

            var validSensors = sensors
                .Where(s => s.Value.HasValue && IsPlausibleTemperature(s.Value.Value))
                .GroupBy(s => $"{s.Hardware.Name}|{s.Name}")
                .Select(g => g.First())
                .ToList();

            if (!validSensors.Any())
            {
                return null;
            }

            string[] preferredNames =
            {
                "cpu package",
                "package",
                "tctl",
                "tdie",
                "core max",
                "core average",
                "ccd"
            };

            foreach (var preferredName in preferredNames)
            {
                var sensor = validSensors.FirstOrDefault(s =>
                    s.Name.Contains(preferredName, StringComparison.OrdinalIgnoreCase));
                if (sensor?.Value is float value)
                {
                    return value;
                }
            }

            var coreSensors = validSensors
                .Where(s => s.Name.Contains("core", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Value!.Value)
                .ToList();

            if (coreSensors.Any())
            {
                return coreSensors.Average();
            }

            return validSensors.Select(s => s.Value!.Value).Average();
        }

        private static IEnumerable<ISensor> GetTemperatureSensors(IHardware hardware)
        {
            foreach (var sensor in hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature))
            {
                yield return sensor;
            }

            foreach (var subHardware in hardware.SubHardware)
            {
                try
                {
                    subHardware.Update();
                }
                catch
                {
                }

                foreach (var sensor in subHardware.Sensors.Where(s => s.SensorType == SensorType.Temperature))
                {
                    yield return sensor;
                }
            }
        }

        private static bool IsLikelyCpuTemperatureSensor(string sensorName)
            => sensorName.Contains("cpu", StringComparison.OrdinalIgnoreCase) ||
               sensorName.Contains("package", StringComparison.OrdinalIgnoreCase) ||
               sensorName.Contains("core", StringComparison.OrdinalIgnoreCase) ||
               sensorName.Contains("tctl", StringComparison.OrdinalIgnoreCase) ||
               sensorName.Contains("tdie", StringComparison.OrdinalIgnoreCase) ||
               sensorName.Contains("ccd", StringComparison.OrdinalIgnoreCase);

        private static bool IsPlausibleTemperature(float value)
            => value is >= 5f and <= 125f;

        private float? TryGetWmiCpuTemperature()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\WMI",
                    "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");

                var temperatures = searcher.Get()
                    .Cast<ManagementObject>()
                    .Select(obj => (Convert.ToSingle(obj["CurrentTemperature"]) / 10f) - 273.15f)
                    .Where(IsPlausibleTemperature)
                    .ToList();

                return temperatures.Any() ? temperatures.Average() : null;
            }
            catch { }

            return null;
        }
        private float? TryGetWmiCpuClock() { try { using var searcher = new ManagementObjectSearcher("select CurrentClockSpeed from Win32_Processor"); foreach (var item in searcher.Get()) return Convert.ToSingle(item["CurrentClockSpeed"]); } catch { } return null; }
        private float? TryGetWmiCpuLoad() { try { using var searcher = new ManagementObjectSearcher("select LoadPercentage from Win32_Processor"); foreach (var item in searcher.Get()) return Convert.ToSingle(item["LoadPercentage"]); } catch { } return null; }

        public (int Processes, int Threads) GetSystemProcessesAndThreads()
        {
            var processes = Process.GetProcesses();
            int threadCount = 0;
            foreach (var p in processes) { try { threadCount += p.Threads.Count; } catch { } }
            return (processes.Length, threadCount);
        }

        public bool TryGetRamUsageGb(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
            if (ram is null)
            {
                reason = "RAM hardware not detected.";
                return false;
            }

            var sensor = ram.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Memory Used" && s.Value.HasValue);
            if (sensor?.Value is float used)
            {
                value = used;
                return true;
            }

            reason = "Memory Used sensor did not return data.";
            return false;
        }

        public float GetRamUsageGb()
            => TryGetRamUsageGb(out var value, out _) ? value : 0f;

        public bool TryGetRamAvailableGb(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
            if (ram is null)
            {
                reason = "RAM hardware not detected.";
                return false;
            }

            var sensor = ram.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Memory Available" && s.Value.HasValue);
            if (sensor?.Value is float available)
            {
                value = available;
                return true;
            }

            reason = "Memory Available sensor did not return data.";
            return false;
        }

        public float GetRamAvailableGb()
            => TryGetRamAvailableGb(out var value, out _) ? value : 0f;

        public bool TryGetRamLoad(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            bool hasUsed = TryGetRamUsageGb(out float used, out string usedReason);
            bool hasAvailable = TryGetRamAvailableGb(out float available, out string availableReason);

            float total = used + available;

            if (hasUsed && hasAvailable && total > 0f)
            {
                value = (used / total) * 100f;
                return true;
            }

            var ram = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
            var loadSensor = ram?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Value.HasValue);
            if (loadSensor?.Value is float load)
            {
                value = load;
                return true;
            }

            reason = $"RAM load unavailable. {usedReason} {availableReason}".Trim();
            return false;
        }

        public float GetRamLoad()
            => TryGetRamLoad(out var value, out _) ? value : 0f;

        public bool TryGetGpuLoad(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var gpu = _computer.Hardware.FirstOrDefault(h =>
                h.HardwareType == HardwareType.GpuNvidia ||
                h.HardwareType == HardwareType.GpuAmd ||
                h.HardwareType == HardwareType.GpuIntel);

            if (gpu is null)
            {
                reason = "GPU hardware not detected.";
                return false;
            }

            var loadSensor = gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Value.HasValue);
            if (loadSensor?.Value is float load)
            {
                value = load;
                return true;
            }

            reason = "GPU load sensor did not return data.";
            return false;
        }

        public float GetGpuLoad()
            => TryGetGpuLoad(out var value, out _) ? value : 0f;

        public bool TryGetGpuTemperature(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var gpu = _computer.Hardware.FirstOrDefault(h =>
                h.HardwareType == HardwareType.GpuNvidia ||
                h.HardwareType == HardwareType.GpuAmd ||
                h.HardwareType == HardwareType.GpuIntel);

            if (gpu is null)
            {
                reason = "GPU hardware not detected.";
                return false;
            }

            var temperatureSensors = gpu.Sensors
                .Where(s => s.SensorType == SensorType.Temperature)
                .ToList();

            if (!temperatureSensors.Any())
            {
                reason = "GPU temperature unsupported on this device.";
                return false;
            }

            var tempSensor = temperatureSensors.FirstOrDefault(s => s.Value.HasValue);
            if (tempSensor?.Value is float temp)
            {
                value = temp;
                return true;
            }

            reason = "GPU temperature sensor detected but did not return data.";
            return false;
        }

        public float GetGpuTemperature()
            => TryGetGpuTemperature(out var value, out _) ? value : 0f;

        public bool TryGetDiskLoad(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var sensorLoads = _computer.Hardware
                .Where(h => h.HardwareType == HardwareType.Storage)
                .SelectMany(GetStorageLoadSensors)
                .Where(s => s.Value.HasValue)
                .Select(s => ClampPercent(s.Value!.Value))
                .ToList();

            if (sensorLoads.Any(load => load > 0.1f))
            {
                value = sensorLoads.Max();
                return true;
            }

            var wmiLoad = TryGetWmiDiskPercentTime();
            if (wmiLoad.HasValue)
            {
                value = wmiLoad.Value;
                return true;
            }

            var readMbit = TryGetDiskReadRateMbit(out var readRate, out _) ? readRate : 0f;
            var writeMbit = TryGetDiskWriteRateMbit(out var writeRate, out _) ? writeRate : 0f;
            var throughputMbit = readMbit + writeMbit;
            if (throughputMbit > 0.5f)
            {
                value = ClampPercent(Math.Max(1f, throughputMbit / 10f));
                return true;
            }

            if (sensorLoads.Any())
            {
                value = 0f;
                return true;
            }

            reason = "Disk load sensor and WMI fallback did not return data.";
            return false;
        }

        public float GetDiskLoad()
            => TryGetDiskLoad(out var value, out _) ? value : 0f;

        public bool TryGetDiskReadRateMbit(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var storage = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Storage);

            if (storage is null)
            {
                reason = "Storage hardware not detected.";
                return false;
            }

            var readSensor = storage.Sensors.FirstOrDefault(s =>
                s.SensorType == SensorType.Throughput &&
                s.Name.Contains("Read", StringComparison.OrdinalIgnoreCase));

            if (readSensor?.Value is float read)
            {
                var sensorMbit = NormalizeSensorThroughputToMbit(read, readSensor.Name);
                if (sensorMbit is >= 0f and <= MaxPlausibleDiskThroughputMbit)
                {
                    value = sensorMbit;
                    return true;
                }
            }

            var wmiRead = TryGetWmiDiskReadBytesPerSec();
            if (wmiRead.HasValue)
            {
                value = (wmiRead.Value * 8f) / 1_000_000f;
                return true;
            }

            reason = "Disk read throughput (Mb/s) sensor and WMI fallback did not return data.";
            return false;
        }

        public float GetDiskReadRateMbit()
            => TryGetDiskReadRateMbit(out var value, out _) ? value : 0f;

        public bool TryGetDiskWriteRateMbit(out float value, out string reason)
        {
            value = 0f;
            reason = string.Empty;

            var storage = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Storage);

            if (storage is null)
            {
                reason = "Storage hardware not detected.";
                return false;
            }

            var writeSensor = storage.Sensors.FirstOrDefault(s =>
                s.SensorType == SensorType.Throughput &&
                s.Name.Contains("Write", StringComparison.OrdinalIgnoreCase));

            if (writeSensor?.Value is float write)
            {
                var sensorMbit = NormalizeSensorThroughputToMbit(write, writeSensor.Name);
                if (sensorMbit is >= 0f and <= MaxPlausibleDiskThroughputMbit)
                {
                    value = sensorMbit;
                    return true;
                }
            }

            var wmiWrite = TryGetWmiDiskWriteBytesPerSec();
            if (wmiWrite.HasValue)
            {
                value = (wmiWrite.Value * 8f) / 1_000_000f;
                return true;
            }

            reason = "Disk write throughput (Mb/s) sensor and WMI fallback did not return data.";
            return false;
        }

        public float GetDiskWriteRateMbit()
            => TryGetDiskWriteRateMbit(out var value, out _) ? value : 0f;

        private float? TryGetWmiDiskPercentTime()
        {
            try
            {
                var physicalDiskValue = TryGetWmiDiskPercentTime(
                    "SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
                if (physicalDiskValue.HasValue)
                {
                    return physicalDiskValue.Value;
                }

                Thread.Sleep(200);

                physicalDiskValue = TryGetWmiDiskPercentTime(
                    "SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
                if (physicalDiskValue.HasValue)
                {
                    return physicalDiskValue.Value;
                }

                return TryGetWmiDiskPercentTime(
                    "SELECT Name, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk");
            }
            catch { }

            return null;
        }

        private static float? TryGetWmiDiskPercentTime(string query)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(query);
                var values = new List<float>();
                float? totalValue = null;

                foreach (var item in searcher.Get())
                {
                    var name = item["Name"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    var rawValue = Convert.ToSingle(item["PercentDiskTime"]);
                    if (float.IsNaN(rawValue) || float.IsInfinity(rawValue) || rawValue < 0f)
                    {
                        continue;
                    }

                    var clampedValue = ClampPercent(rawValue);
                    if (name.Equals("_Total", StringComparison.OrdinalIgnoreCase))
                    {
                        totalValue = clampedValue;
                    }
                    else
                    {
                        values.Add(clampedValue);
                    }
                }

                return values.Any() ? values.Max() : totalValue;
            }
            catch { }

            return null;
        }

        private static IEnumerable<ISensor> GetStorageLoadSensors(IHardware storage)
        {
            try
            {
                storage.Update();
            }
            catch
            {
            }

            foreach (var sensor in storage.Sensors.Where(s => s.SensorType == SensorType.Load))
            {
                yield return sensor;
            }

            foreach (var subHardware in storage.SubHardware)
            {
                try
                {
                    subHardware.Update();
                }
                catch
                {
                }

                foreach (var sensor in subHardware.Sensors.Where(s => s.SensorType == SensorType.Load))
                {
                    yield return sensor;
                }
            }
        }

        private static float ClampPercent(float value)
            => Math.Clamp(value, 0f, 100f);

        private float? TryGetWmiDiskReadBytesPerSec()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT DiskReadBytesPerSec FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk WHERE Name = '_Total'");

                foreach (var item in searcher.Get())
                {
                    return Convert.ToSingle(item["DiskReadBytesPerSec"]);
                }
            }
            catch { }

            return null;
        }

        private float? TryGetWmiDiskWriteBytesPerSec()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT DiskWriteBytesPerSec FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk WHERE Name = '_Total'");

                foreach (var item in searcher.Get())
                {
                    return Convert.ToSingle(item["DiskWriteBytesPerSec"]);
                }
            }
            catch { }

            return null;
        }

        private static float NormalizeSensorThroughputToMbit(float rawValue, string sensorName)
        {
            if (rawValue < 0f)
            {
                return -1f;
            }

            // Prefer explicit unit hints when present in the sensor name.
            if (sensorName.Contains("GB/s", StringComparison.OrdinalIgnoreCase))
            {
                return rawValue * 8_000f;
            }

            if (sensorName.Contains("MB/s", StringComparison.OrdinalIgnoreCase))
            {
                return rawValue * 8f;
            }

            if (sensorName.Contains("KB/s", StringComparison.OrdinalIgnoreCase) ||
                sensorName.Contains("KiB/s", StringComparison.OrdinalIgnoreCase))
            {
                return (rawValue * 8f) / 1_000f;
            }

            if (sensorName.Contains("B/s", StringComparison.OrdinalIgnoreCase))
            {
                return (rawValue * 8f) / 1_000_000f;
            }

            // Default for LibreHardwareMonitor throughput sensors.
            return (rawValue * 8f) / 1_000f;
        }

        public void Dispose() => _computer.Close();
    }
}
