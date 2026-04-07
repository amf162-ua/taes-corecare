using System;
using System.Linq;
using System.Threading.Tasks;
using CoreCare.Models;
using CoreCare.Services;

namespace CoreCare.Orchestrators
{
    public class BenchmarkOrchestrator
    {
        private readonly HardwareMonitorService _monitor;
        private readonly Models.StressWorker _stressWorker;

        public BenchmarkOrchestrator(HardwareMonitorService monitor, Models.StressWorker stressWorker)
        {
            _monitor = monitor;
            _stressWorker = stressWorker;
        }

        public async Task<RegistroBenchmark> RunBenchmarkAsync(ScanOptions options, int durationSeconds)
        {
            var result = new BenchmarkResult();
            var startTime = DateTime.Now;

            var cpuTemps = new System.Collections.Generic.List<float>();
            var cpuLoads = new System.Collections.Generic.List<float>();
            var cpuClocks = new System.Collections.Generic.List<float>();
            var gpuTemps = new System.Collections.Generic.List<float>();
            var gpuLoads = new System.Collections.Generic.List<float>();
            var ramUsed = new System.Collections.Generic.List<float>();
            var ramLoads = new System.Collections.Generic.List<float>();
            var diskLoads = new System.Collections.Generic.List<float>();
            var diskReads = new System.Collections.Generic.List<float>();
            var diskWrites = new System.Collections.Generic.List<float>();

            static void AddReading(
                System.Collections.Generic.List<SensorReading> bucket,
                ComponentType component,
                SensorType type,
                string name,
                float value)
            {
                bucket.Add(new SensorReading
                {
                    Component = component,
                    Type = type,
                    Name = name,
                    Value = value,
                    TimeStamp = DateTime.Now
                });
            }

            // Baseline readings
            for (int i = 0; i < 3; i++)
            {
                _monitor.UpdateHardware();

                if (options.ScanCPU)
                {
                    var cpuTempData = _monitor.GetCpuTemperature();
                    var cpuLoad = _monitor.GetCpuLoad();
                    var cpuClock = _monitor.GetCpuClockGHz();

                    AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp", cpuTempData.Value);
                    AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Load, "CPU Load", cpuLoad);
                    AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock", cpuClock);
                }

                if (options.ScanGPU)
                {
                    var gpuTemp = _monitor.GetGpuTemperature();
                    var gpuLoad = _monitor.GetGpuLoad();

                    AddReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp", gpuTemp);
                    AddReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Load, "GPU Load", gpuLoad);
                }

                if (options.ScanRAM)
                {
                    var used = _monitor.GetRamUsageGb();
                    var load = _monitor.GetRamLoad();

                    AddReading(result.BaselineReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", used);
                    AddReading(result.BaselineReadings, ComponentType.Ram, SensorType.Load, "RAM Load", load);
                }

                if (options.ScanDisk)
                {
                    var diskLoad = _monitor.GetDiskLoad();
                    var readRate = _monitor.GetDiskReadRateMb();
                    var writeRate = _monitor.GetDiskWriteRateMb();

                    AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Load, "Disk Load", diskLoad);
                    AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read MB/s", readRate);
                    AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write MB/s", writeRate);
                }

                await Task.Delay(1000);
            }

            // Stress phase
            if (options.ScanCPU) _stressWorker.RunCpuStress(durationSeconds);
            if (options.ScanDisk) _stressWorker.RunDiskStress(durationSeconds);
            if (options.ScanRAM) _stressWorker.RunRamStress(durationSeconds);

            DateTime endTime = DateTime.Now.AddSeconds(durationSeconds);

            while (DateTime.Now < endTime)
            {
                _monitor.UpdateHardware();

                if (options.ScanCPU)
                {
                    var cpuTempData = _monitor.GetCpuTemperature();
                    var cpuLoad = _monitor.GetCpuLoad();
                    var cpuClock = _monitor.GetCpuClockGHz();

                    cpuTemps.Add(cpuTempData.Value);
                    cpuLoads.Add(cpuLoad);
                    cpuClocks.Add(cpuClock);

                    AddReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp", cpuTempData.Value);
                    AddReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Load, "CPU Load", cpuLoad);
                    AddReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock", cpuClock);
                }

                if (options.ScanGPU)
                {
                    var gpuTemp = _monitor.GetGpuTemperature();
                    var gpuLoad = _monitor.GetGpuLoad();

                    gpuTemps.Add(gpuTemp);
                    gpuLoads.Add(gpuLoad);

                    AddReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp", gpuTemp);
                    AddReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Load, "GPU Load", gpuLoad);
                }

                if (options.ScanRAM)
                {
                    var used = _monitor.GetRamUsageGb();
                    var load = _monitor.GetRamLoad();

                    ramUsed.Add(used);
                    ramLoads.Add(load);

                    AddReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", used);
                    AddReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Load, "RAM Load", load);
                }

                if (options.ScanDisk)
                {
                    var diskLoad = _monitor.GetDiskLoad();
                    var readRate = _monitor.GetDiskReadRateMb();
                    var writeRate = _monitor.GetDiskWriteRateMb();

                    diskLoads.Add(diskLoad);
                    diskReads.Add(readRate);
                    diskWrites.Add(writeRate);

                    AddReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Load, "Disk Load", diskLoad);
                    AddReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read MB/s", readRate);
                    AddReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write MB/s", writeRate);
                }

                await Task.Delay(1000);
            }

            _stressWorker.Stop();

            result.PeakCpuTemp = cpuTemps.Any() ? cpuTemps.Max() : 0f;
            result.PeakGpuTemp = gpuTemps.Any() ? gpuTemps.Max() : 0f;
            result.AvgCpuLoad = cpuLoads.Any() ? cpuLoads.Average() : 0f;
            result.AvgRamLoad = ramLoads.Any() ? ramLoads.Average() : 0f;
            result.AvgDiskLoad = diskLoads.Any() ? diskLoads.Average() : 0f;
            result.Duration = DateTime.Now - startTime;

            float score = 10f;

            if (options.ScanCPU)
            {
                score -= Math.Min(3f, result.PeakCpuTemp / 100f * 3f);
                score -= Math.Min(2f, result.AvgCpuLoad / 100f * 2f);
            }

            if (options.ScanGPU)
            {
                score -= Math.Min(2f, result.PeakGpuTemp / 100f * 2f);
                score -= Math.Min(1.5f, gpuLoads.Any() ? gpuLoads.Average() / 100f * 1.5f : 0f);
            }

            if (options.ScanRAM)
            {
                score -= Math.Min(2f, result.AvgRamLoad / 100f * 2f);
            }

            if (options.ScanDisk)
            {
                score -= Math.Min(1.5f, result.AvgDiskLoad / 100f * 1.5f);
            }

            result.Score = Math.Max(0f, Math.Min(10f, score));

            // Construcción del registro final para la base de datos
            var registroFinal = new RegistroBenchmark
            {
                Timestamp = DateTime.Now,
                CpuTemp = cpuTemps.Any() ? cpuTemps.Average() : 0f,
                CpuLoad = result.AvgCpuLoad,
                CpuClock = cpuClocks.Any() ? cpuClocks.Average() : 0f,
                GpuTemp = gpuTemps.Any() ? gpuTemps.Average() : 0f,
                GpuLoad = gpuLoads.Any() ? gpuLoads.Average() : 0f,
                RamUsed = ramUsed.Any() ? ramUsed.Average() : 0f,
                RamLoad = result.AvgRamLoad,
                DiskLoad = result.AvgDiskLoad,
                DiskReadRate = diskReads.Any() ? diskReads.Average() : 0f,
                DiskWriteRate = diskWrites.Any() ? diskWrites.Average() : 0f,
                Score = result.Score
            };

            return registroFinal;
        }
    }
}