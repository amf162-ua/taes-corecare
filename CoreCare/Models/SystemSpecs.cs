using System.Text.Json.Serialization;

namespace CoreCare.Models
{
    public class SystemSpecs
    {
        [JsonPropertyName("cpu_name")]
        public string CpuName { get; set; } = "N/A";

        [JsonPropertyName("cpu_manufacturer")]
        public string CpuManufacturer { get; set; } = "N/A";

        [JsonPropertyName("cpu_cores")]
        public int CpuCores { get; set; }

        [JsonPropertyName("cpu_threads")]
        public int CpuThreads { get; set; }

        [JsonPropertyName("cpu_max_clock_speed")]
        public string CpuMaxClockSpeed { get; set; } = "N/A";

        [JsonPropertyName("cpu_cache_l2")]
        public string CpuCacheL2 { get; set; } = "N/A";

        [JsonPropertyName("cpu_cache_l3")]
        public string CpuCacheL3 { get; set; } = "N/A";

        [JsonPropertyName("gpu_name")]
        public string GpuName { get; set; } = "N/A";

        [JsonPropertyName("gpu_driver_version")]
        public string GpuDriverVersion { get; set; } = "N/A";

        [JsonPropertyName("gpu_vram")]
        public string GpuVram { get; set; } = "N/A";

        [JsonPropertyName("gpu_processor")]
        public string GpuProcessor { get; set; } = "N/A";

        [JsonPropertyName("ram_total_gb")]
        public double RamTotalGb { get; set; }

        [JsonPropertyName("disk_model")]
        public string DiskModel { get; set; } = "N/A";

        [JsonPropertyName("disk_size")]
        public string DiskSize { get; set; } = "N/A";
    }
}