using System;
using System.Linq;
using System.Management;
using CoreCare.Models;
using Microsoft.Win32;

namespace CoreCare.Services
{
    public class SystemSpecsService : IDisposable
    {
        public SystemSpecs GetSystemSpecs()
        {
            var specs = new SystemSpecs();

            GetCpuInfo(specs);
            GetRamInfo(specs);
            GetGpuInfo(specs);
            GetDiskInfo(specs);

            return specs;
        }

        private void GetCpuInfo(SystemSpecs specs)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        specs.CpuName = obj["Name"]?.ToString() ?? "N/A";
                        specs.CpuManufacturer = obj["Manufacturer"]?.ToString() ?? "N/A";
                        specs.CpuCores = Convert.ToInt32(obj["NumberOfCores"]);
                        specs.CpuThreads = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                        specs.CpuMaxClockMhz = obj["MaxClockSpeed"] == null ? 0 : Convert.ToInt32(obj["MaxClockSpeed"]);
                        specs.CpuMaxClockSpeed = specs.CpuMaxClockMhz > 0 ? $"{specs.CpuMaxClockMhz} MHz" : "N/A";

                        uint l2Kb = Convert.ToUInt32(obj["L2CacheSize"]);
                        specs.CpuCacheL2 = l2Kb >= 1024 ? $"{(l2Kb / 1024.0):F0} MB" : $"{l2Kb} KB";

                        uint l3Kb = Convert.ToUInt32(obj["L3CacheSize"]);
                        specs.CpuCacheL3 = l3Kb >= 1024 ? $"{(l3Kb / 1024.0):F1} MB" : $"{l3Kb} KB";
                        break;
                    }
                }
            }
            catch { }
        }

        private void GetRamInfo(SystemSpecs specs)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get())
                {
                    var totalMemory = obj["TotalPhysicalMemory"];
                    if (totalMemory != null)
                    {
                        specs.RamTotalGb = Math.Round(Convert.ToDouble(totalMemory) / (1024.0 * 1024.0 * 1024.0), 1);
                    }

                    break;
                }
            }
            catch { }
        }

        private void GetGpuInfo(SystemSpecs specs)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var currentBits = obj["CurrentBitsPerPixel"];
                        if (currentBits != null && Convert.ToUInt32(currentBits) > 0)
                        {
                            specs.GpuName = obj["Name"]?.ToString() ?? "N/A";
                            specs.GpuDriverVersion = obj["DriverVersion"]?.ToString() ?? "N/A";

                            var vram = obj["AdapterRAM"];
                            if (vram != null)
                            {
                                ulong vramBytes = Convert.ToUInt64(vram);
                                ApplyGpuVram(specs, vramBytes);
                            }

                            specs.GpuProcessor = obj["VideoProcessor"]?.ToString() ?? "N/A";
                            ImproveGpuVramFromRegistryOrModel(specs);
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        private void GetDiskInfo(SystemSpecs specs)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        specs.DiskModel = obj["Model"]?.ToString() ?? "N/A";
                        specs.DiskMediaType = obj["MediaType"]?.ToString() ?? "N/A";
                        specs.DiskInterfaceType = obj["InterfaceType"]?.ToString() ?? "N/A";
                        var pnpDeviceId = obj["PNPDeviceID"]?.ToString() ?? string.Empty;
                        var size = obj["Size"];
                        if (size != null)
                        {
                            ulong diskBytes = Convert.ToUInt64(size);
                            specs.DiskSizeGb = Math.Round(diskBytes / (1000.0 * 1000.0 * 1000.0), 0);
                            specs.DiskSize = diskBytes >= (1000 * 1000 * 1000)
                                ? $"{(diskBytes / (1000.0 * 1000.0 * 1000.0)):F0} GB"
                                : $"{(diskBytes / (1000.0 * 1000.0)):F0} MB";
                        }

                        ImproveDiskType(specs, pnpDeviceId);
                        break;
                    }
                }
            }
            catch { }
        }

        private static void ImproveGpuVramFromRegistryOrModel(SystemSpecs specs)
        {
            var registryBytes = TryGetGpuMemoryBytesFromRegistry(specs.GpuName);
            if (registryBytes.HasValue && registryBytes.Value > 0)
            {
                ApplyGpuVram(specs, registryBytes.Value);
                return;
            }

            var modelGb = EstimateGpuVramGbFromModel(specs.GpuName);
            if (modelGb.HasValue && modelGb.Value > specs.GpuVramGb)
            {
                ApplyGpuVram(specs, (ulong)(modelGb.Value * 1024 * 1024 * 1024));
            }
        }

        private static ulong? TryGetGpuMemoryBytesFromRegistry(string gpuName)
        {
            if (string.IsNullOrWhiteSpace(gpuName) || gpuName == "N/A")
            {
                return null;
            }

            try
            {
                using var videoRoot = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Video");
                if (videoRoot == null)
                {
                    return null;
                }

                foreach (var adapterKeyName in videoRoot.GetSubKeyNames())
                {
                    using var adapterKey = videoRoot.OpenSubKey(adapterKeyName + @"\0000");
                    if (adapterKey == null)
                    {
                        continue;
                    }

                    var adapterString = adapterKey.GetValue("HardwareInformation.AdapterString")?.ToString() ?? string.Empty;
                    if (!NamesLikelyMatch(gpuName, adapterString))
                    {
                        continue;
                    }

                    var memoryValue = adapterKey.GetValue("HardwareInformation.qwMemorySize");
                    if (memoryValue is long signedValue && signedValue > 0)
                    {
                        return (ulong)signedValue;
                    }

                    if (memoryValue is ulong unsignedValue && unsignedValue > 0)
                    {
                        return unsignedValue;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static bool NamesLikelyMatch(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            var normalizedLeft = NormalizeName(left);
            var normalizedRight = NormalizeName(right);
            return normalizedLeft.Contains(normalizedRight, StringComparison.OrdinalIgnoreCase) ||
                   normalizedRight.Contains(normalizedLeft, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeName(string value)
            => value
                .Replace("NVIDIA", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("GeForce", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("Laptop GPU", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("GPU", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim();

        private static double? EstimateGpuVramGbFromModel(string gpuName)
        {
            if (gpuName.Contains("RTX 4090 Laptop", StringComparison.OrdinalIgnoreCase)) return 16;
            if (gpuName.Contains("RTX 4080 Laptop", StringComparison.OrdinalIgnoreCase)) return 12;
            if (gpuName.Contains("RTX 4070 Laptop", StringComparison.OrdinalIgnoreCase)) return 8;
            if (gpuName.Contains("RTX 3070", StringComparison.OrdinalIgnoreCase)) return 8;
            if (gpuName.Contains("RTX 3080", StringComparison.OrdinalIgnoreCase)) return 10;
            if (gpuName.Contains("RTX 3060", StringComparison.OrdinalIgnoreCase)) return 6;
            if (gpuName.Contains("RTX 2060", StringComparison.OrdinalIgnoreCase)) return 6;
            return null;
        }

        private static void ApplyGpuVram(SystemSpecs specs, ulong vramBytes)
        {
            specs.GpuVramGb = Math.Round(vramBytes / (1024.0 * 1024.0 * 1024.0), 1);
            specs.GpuVram = vramBytes >= (1024UL * 1024UL * 1024UL)
                ? $"{(vramBytes / (1024.0 * 1024.0 * 1024.0)):F0} GB"
                : $"{(vramBytes / (1024.0 * 1024.0)):F0} MB";
        }

        private static void ImproveDiskType(SystemSpecs specs, string pnpDeviceId)
        {
            ApplyPhysicalDiskInfo(specs);

            var combined = $"{specs.DiskModel} {specs.DiskMediaType} {specs.DiskInterfaceType} {pnpDeviceId}";
            if (combined.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("MTFD", StringComparison.OrdinalIgnoreCase))
            {
                specs.DiskMediaType = "SSD";
                specs.DiskInterfaceType = "NVMe";
                return;
            }

            if (combined.Contains("SSD", StringComparison.OrdinalIgnoreCase))
            {
                specs.DiskMediaType = "SSD";
            }
        }

        private static void ApplyPhysicalDiskInfo(SystemSpecs specs)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\Microsoft\Windows\Storage",
                    "SELECT FriendlyName, Model, MediaType, BusType, Size FROM MSFT_PhysicalDisk");

                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    var friendlyName = obj["FriendlyName"]?.ToString() ?? string.Empty;
                    var model = obj["Model"]?.ToString() ?? string.Empty;
                    if (!NamesLikelyMatch(specs.DiskModel, friendlyName) && !NamesLikelyMatch(specs.DiskModel, model))
                    {
                        continue;
                    }

                    var mediaType = obj["MediaType"] == null ? 0 : Convert.ToUInt16(obj["MediaType"]);
                    var busType = obj["BusType"] == null ? 0 : Convert.ToUInt16(obj["BusType"]);

                    if (mediaType == 4 || mediaType == 5)
                    {
                        specs.DiskMediaType = mediaType == 4 ? "SSD" : "SCM";
                    }
                    else if (mediaType == 3)
                    {
                        specs.DiskMediaType = "HDD";
                    }

                    if (busType == 17)
                    {
                        specs.DiskInterfaceType = "NVMe";
                    }
                    else if (busType == 11)
                    {
                        specs.DiskInterfaceType = "SATA";
                    }

                    return;
                }
            }
            catch
            {
            }
        }

        public void Dispose() { }
    }
}
