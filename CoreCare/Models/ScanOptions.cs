using System;
using System.Collections.Generic;
using System.Text;

// Controla qué componentes se escanean.
// Se pasa como parámetro a HardwareMonitorService.
namespace CoreCare.Models
{
    public class ScanOptions
    {
        public bool ScanCPU { get; set; }
        public bool ScanGPU { get; set; }
        public bool ScanRAM { get; set; }
        public bool ScanDisk { get; set; }

        public static ScanOptions FullScan() => new() { ScanCPU = true, ScanGPU = true, ScanRAM = true, ScanDisk = true };
        public static ScanOptions ScanCPUOnly() => new() { ScanCPU = true };
        public static ScanOptions ScanGPUOnly() => new() { ScanGPU = true };
        public static ScanOptions ScanRAMOnly() => new() { ScanRAM = true };
        public static ScanOptions ScanDiskOnly() => new() { ScanDisk = true };
    }
}
