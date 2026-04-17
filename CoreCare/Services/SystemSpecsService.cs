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
                        specs.CpuMaxClockSpeed = obj["MaxClockSpeed"]?.ToString() + " MHz";

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
                        var size = obj["Size"];
                        if (size != null)
                        {
                            ulong diskBytes = Convert.ToUInt64(size);
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