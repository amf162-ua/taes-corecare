using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var globalStopwatch = Stopwatch.StartNew();

            var cpuTemps = new List<float>();
            var cpuLoads = new List<float>();
            var cpuClocks = new List<float>();
            var gpuTemps = new List<float>();
            var gpuLoads = new List<float>();
            var ramUsed = new List<float>();
            var ramLoads = new List<float>();
            var diskLoads = new List<float>();
            var diskReads = new List<float>();
            var diskWrites = new List<float>();

            static void AddReading(
                List<SensorReading> bucket,
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

            // Baseline: 3 lecturas en reposo
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _monitor.UpdateHardware();

                    if (options.ScanCPU)
                    {
                        try
                        {
                            var cpuTempData = _monitor.GetCpuTemperature();
                            var cpuLoad = _monitor.GetCpuLoad();
                            var cpuClock = _monitor.GetCpuClockGHz();

                            AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp", cpuTempData.Value);
                            AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Load, "CPU Load", cpuLoad);
                            AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock", cpuClock);
                        }
                        catch { /* CPU data unavailable, continue */ }
                    }

                    if (options.ScanGPU)
                    {
                        try
                        {
                            var gpuTemp = _monitor.GetGpuTemperature();
                            var gpuLoad = _monitor.GetGpuLoad();

                            AddReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp", gpuTemp);
                            AddReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Load, "GPU Load", gpuLoad);
                        }
                        catch { /* GPU data unavailable, continue */ }
                    }

                    if (options.ScanRAM)
                    {
                        try
                        {
                            var used = _monitor.GetRamUsageGb();
                            var load = _monitor.GetRamLoad();

                            AddReading(result.BaselineReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", used);
                            AddReading(result.BaselineReadings, ComponentType.Ram, SensorType.Load, "RAM Load", load);
                        }
                        catch { /* RAM data unavailable, continue */ }
                    }

                    if (options.ScanDisk)
                    {
                        try
                        {
                            var diskLoad = _monitor.GetDiskLoad();
                            var readRate = _monitor.GetDiskReadRateMb();
                            var writeRate = _monitor.GetDiskWriteRateMb();

                            AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Load, "Disk Load", diskLoad);
                            AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read MB/s", readRate);
                            AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write MB/s", writeRate);
                        }
                        catch { /* Disk data unavailable, continue */ }
                    }
                }
                catch { /* General error in baseline reading, continue */ }

                await Task.Delay(1000);
            }

            // Stress phase: inicia la carga
            try
            {
                if (options.ScanCPU) _stressWorker.RunCpuStress(durationSeconds);
                if (options.ScanDisk) _stressWorker.RunDiskStress(durationSeconds);
                if (options.ScanRAM) _stressWorker.RunRamStress(durationSeconds);
            }
            catch { /* Stress worker error, continue with monitoring */ }

            var stressStopwatch = Stopwatch.StartNew();

            // Monitoreo bajo carga con Stopwatch preciso
            while (stressStopwatch.Elapsed < TimeSpan.FromSeconds(durationSeconds))
            {
                try
                {
                    _monitor.UpdateHardware();

                    if (options.ScanCPU)
                    {
                        try
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
                        catch { /* CPU data unavailable, continue */ }
                    }

                    if (options.ScanGPU)
                    {
                        try
                        {
                            var gpuTemp = _monitor.GetGpuTemperature();
                            var gpuLoad = _monitor.GetGpuLoad();

                            gpuTemps.Add(gpuTemp);
                            gpuLoads.Add(gpuLoad);

                            AddReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp", gpuTemp);
                            AddReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Load, "GPU Load", gpuLoad);
                        }
                        catch { /* GPU data unavailable, continue */ }
                    }

                    if (options.ScanRAM)
                    {
                        try
                        {
                            var used = _monitor.GetRamUsageGb();
                            var load = _monitor.GetRamLoad();

                            ramUsed.Add(used);
                            ramLoads.Add(load);

                            AddReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", used);
                            AddReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Load, "RAM Load", load);
                        }
                        catch { /* RAM data unavailable, continue */ }
                    }

                    if (options.ScanDisk)
                    {
                        try
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
                        catch { /* Disk data unavailable, continue */ }
                    }
                }
                catch { /* General error in stress reading, continue */ }

                await Task.Delay(1000);
            }

            stressStopwatch.Stop();
            globalStopwatch.Stop();

            try
            {
                _stressWorker.Stop();
            }
            catch { /* Error stopping stress worker */ }

            result.PeakCpuTemp = cpuTemps.Any() ? cpuTemps.Max() : 0f;
            result.PeakGpuTemp = gpuTemps.Any() ? gpuTemps.Max() : 0f;
            result.AvgCpuLoad = cpuLoads.Any() ? cpuLoads.Average() : 0f;
            result.AvgRamLoad = ramLoads.Any() ? ramLoads.Average() : 0f;
            result.AvgDiskLoad = diskLoads.Any() ? diskLoads.Average() : 0f;
            result.Duration = globalStopwatch.Elapsed;

            // Cálculo de score normalizado según componentes escaneados
            float score = 10f;
            int componentCount = 0;
            float maxPossibleDeduction = 0f;

            if (options.ScanCPU)
            {
                componentCount++;
                score -= Math.Min(3f, Math.Max(0, result.PeakCpuTemp / 100f * 3f));
                score -= Math.Min(2f, Math.Max(0, result.AvgCpuLoad / 100f * 2f));
                maxPossibleDeduction += 5f;
            }

            if (options.ScanGPU)
            {
                componentCount++;
                score -= Math.Min(2f, Math.Max(0, result.PeakGpuTemp / 100f * 2f));
                score -= Math.Min(1.5f, gpuLoads.Any() ? Math.Max(0, gpuLoads.Average() / 100f * 1.5f) : 0f);
                maxPossibleDeduction += 3.5f;
            }

            if (options.ScanRAM)
            {
                componentCount++;
                score -= Math.Min(2f, Math.Max(0, result.AvgRamLoad / 100f * 2f));
                maxPossibleDeduction += 2f;
            }

            if (options.ScanDisk)
            {
                componentCount++;
                score -= Math.Min(1.5f, Math.Max(0, result.AvgDiskLoad / 100f * 1.5f));
                maxPossibleDeduction += 1.5f;
            }

            // Normalizar score si no se escanean todos los componentes
            if (componentCount > 0 && maxPossibleDeduction > 0)
            {
                float deduction = 10f - score;
                float scaleFactor = 10.5f / maxPossibleDeduction;  // 10.5 es la suma de todos los máximos posibles
                score = 10f - (deduction * scaleFactor);
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

            // Agregar todas las lecturas de sensores (baseline + bajo carga)
            foreach (var reading in result.BaselineReadings)
            {
                reading.RegistroBenchmark = registroFinal;
                registroFinal.SensorReadings.Add(reading);
            }

            foreach (var reading in result.UnderLoadReadings)
            {
                reading.RegistroBenchmark = registroFinal;
                registroFinal.SensorReadings.Add(reading);
            }

            return registroFinal;
        }
    }
}