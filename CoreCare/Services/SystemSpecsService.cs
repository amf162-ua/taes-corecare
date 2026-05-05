using System;
using System.Management;
using CoreCare.Models;

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
                                specs.GpuVramGb = Math.Round(vramBytes / (1024.0 * 1024.0 * 1024.0), 1);
                                specs.GpuVram = vramBytes >= (1024 * 1024 * 1024)
                                    ? $"{(vramBytes / (1024.0 * 1024.0 * 1024.0)):F0} GB"
                                    : $"{(vramBytes / (1024.0 * 1024.0)):F0} MB";
                            }

                            specs.GpuProcessor = obj["VideoProcessor"]?.ToString() ?? "N/A";
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
                        var size = obj["Size"];
                        if (size != null)
                        {
                            ulong diskBytes = Convert.ToUInt64(size);
                            specs.DiskSizeGb = Math.Round(diskBytes / (1000.0 * 1000.0 * 1000.0), 0);
                            specs.DiskSize = diskBytes >= (1000 * 1000 * 1000)
                                ? $"{(diskBytes / (1000.0 * 1000.0 * 1000.0)):F0} GB"
                                : $"{(diskBytes / (1000.0 * 1000.0)):F0} MB";
                        }
                        break;
                    }
                }
            }
            catch { }
        }

        public void Dispose() { }
    }
}
