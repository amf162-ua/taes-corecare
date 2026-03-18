using System;
using System.Collections.Generic;
using System.Text;

// Objeto temporal con todos los datos de un benchmark (lecturas, métricas y Score).
// No se persiste directamente — para eso existe RegistroBenchmark
namespace CoreCare.Models
{
    public class BenchmarkResult
    {
        public List<SensorReading> BaselineReadings { get; set; } = new();
        public List<SensorReading> UnderLoadReadings { get; set; } = new();
        public float PeakCpuTemp { get; set; }
        public float PeakGpuTemp { get; set; }
        public float AvgCpuLoad { get; set; }
        public float AvgRamLoad { get; set; }
        public float AvgDiskLoad { get; set; }
        public float Score { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
