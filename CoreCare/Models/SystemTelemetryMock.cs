using System.Text.Json.Serialization;

namespace CoreCare.Models
{
    public class SystemTelemetryMock
    {
        [JsonPropertyName("cpu_usage_percent")]
        public double CpuUsagePercent { get; set; }

        [JsonPropertyName("gpu_usage_percent")]
        public double GpuUsagePercent { get; set; }

        [JsonPropertyName("cpu_temperature_c")]
        public double CpuTemperatureC { get; set; }

        [JsonPropertyName("gpu_temperature_c")]
        public double GpuTemperatureC { get; set; }

        [JsonPropertyName("ram_total_gb")]
        public double RamTotalGb { get; set; }

        [JsonPropertyName("ram_used_gb")]
        public double RamUsedGb { get; set; }

        [JsonPropertyName("disk_type")]
        public string DiskType { get; set; } = string.Empty; // Ej: "HDD" o "SSD"

        [JsonPropertyName("disk_usage_percent")]
        public double DiskUsagePercent { get; set; }
    }
}
