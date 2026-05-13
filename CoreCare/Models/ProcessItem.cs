using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCare.Models
{
    public class ProcessItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double RamUsageMB { get; set; }
        public double CpuUsagePercent { get; set; }
        public bool IsHighConsumption => CpuUsagePercent > 15 || RamUsageMB > 500;
        public bool IsCritical { get; set; } = false;
    }
}
