using System;
using System.Collections.Generic;
using System.Text;

//para guardar y consultar el historial de rendimiento del equipo a lo largo del tiempo.
namespace CoreCare.Models
{
    public class RegistroBenchmark
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public float CpuLoad { get; set; }
        public float CpuTemp { get; set; }
        public float CpuClock { get; set; }
        public float GpuTemp { get; set; }
        public float GpuLoad { get; set; }
        public float RamUsed { get; set; }
        public float RamLoad { get; set; }
        public float DiskLoad { get; set; }
        public float DiskReadRate { get; set; }
        public float DiskWriteRate { get; set; }
        public float Score { get; set; }
    }
}
