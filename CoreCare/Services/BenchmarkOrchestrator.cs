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
        private readonly TelemetryValidationService _telemetryValidation = new();

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

            void AddUnavailableReading(
                List<SensorReading> bucket,
                ComponentType component,
                SensorType type,
                string name,
                string reason)
            {
                bucket.Add(new SensorReading
                {
                    Component = component,
                    Type = type,
                    Name = $"{name} - NO DISPONIBLE ({reason})",
                    Value = 0,
                    TimeStamp = DateTime.Now
                });
            }

            bool AddReading(
                List<SensorReading> bucket,
                ComponentType component,
                SensorType type,
                string name,
                float value)
            {
                if (!_telemetryValidation.TryValidate(component, type, name, value, out var validationError))
                {
                    AddUnavailableReading(bucket, component, type, name, validationError);
                    return false;
                }

                bucket.Add(new SensorReading
                {
                    Component = component,
                    Type = type,
                    Name = name,
                    Value = value,
                    TimeStamp = DateTime.Now
                });

                return true;
            }

            // Baseline: 3 lecturas en reposo
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _monitor.UpdateHardware();

                    if (options.ScanCPU)
                    {
                        if (_monitor.TryGetCpuTemperature(out var cpuTempData, out var cpuTempReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp C", cpuTempData.Value);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp C", cpuTempReason);
                        }

                        if (_monitor.TryGetCpuLoad(out var cpuLoad, out var cpuLoadReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Load, "CPU Load %", cpuLoad);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Load, "CPU Load %", cpuLoadReason);
                        }

                        if (_monitor.TryGetCpuClockGHz(out var cpuClock, out var cpuClockReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock GHz", cpuClock);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock GHz", cpuClockReason);
                        }
                    }

                    if (options.ScanGPU)
                    {
                        if (_monitor.TryGetGpuTemperature(out var gpuTemp, out var gpuTempReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp C", gpuTemp);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp C", gpuTempReason);
                        }

                        if (_monitor.TryGetGpuLoad(out var gpuLoad, out var gpuLoadReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Load, "GPU Load %", gpuLoad);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Gpu, SensorType.Load, "GPU Load %", gpuLoadReason);
                        }
                    }

                    if (options.ScanRAM)
                    {
                        if (_monitor.TryGetRamUsageGb(out var used, out var ramUsedReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", used);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", ramUsedReason);
                        }

                        if (_monitor.TryGetRamLoad(out var load, out var ramLoadReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Ram, SensorType.Load, "RAM Load %", load);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Ram, SensorType.Load, "RAM Load %", ramLoadReason);
                        }
                    }

                    if (options.ScanDisk)
                    {
                        if (_monitor.TryGetDiskLoad(out var diskLoad, out var diskLoadReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Load, "Disk Load %", diskLoad);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Disk, SensorType.Load, "Disk Load %", diskLoadReason);
                        }

                        if (_monitor.TryGetDiskReadRateMbit(out var readRate, out var diskReadReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read Mb/s", readRate);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read Mb/s", diskReadReason);
                        }

                        if (_monitor.TryGetDiskWriteRateMbit(out var writeRate, out var diskWriteReason))
                        {
                            AddReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write Mb/s", writeRate);
                        }
                        else
                        {
                            AddUnavailableReading(result.BaselineReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write Mb/s", diskWriteReason);
                        }
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
                        if (_monitor.TryGetCpuTemperature(out var cpuTempData, out var cpuTempReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp C", cpuTempData.Value))
                            {
                                cpuTemps.Add(cpuTempData.Value);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Temperature, "CPU Temp C", cpuTempReason);
                        }

                        if (_monitor.TryGetCpuLoad(out var cpuLoad, out var cpuLoadReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Load, "CPU Load %", cpuLoad))
                            {
                                cpuLoads.Add(cpuLoad);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Load, "CPU Load %", cpuLoadReason);
                        }

                        if (_monitor.TryGetCpuClockGHz(out var cpuClock, out var cpuClockReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock GHz", cpuClock))
                            {
                                cpuClocks.Add(cpuClock);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Cpu, SensorType.Clock, "CPU Clock GHz", cpuClockReason);
                        }
                    }

                    if (options.ScanGPU)
                    {
                        if (_monitor.TryGetGpuTemperature(out var gpuTemp, out var gpuTempReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp C", gpuTemp))
                            {
                                gpuTemps.Add(gpuTemp);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Temperature, "GPU Temp C", gpuTempReason);
                        }

                        if (_monitor.TryGetGpuLoad(out var gpuLoad, out var gpuLoadReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Load, "GPU Load %", gpuLoad))
                            {
                                gpuLoads.Add(gpuLoad);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Gpu, SensorType.Load, "GPU Load %", gpuLoadReason);
                        }
                    }

                    if (options.ScanRAM)
                    {
                        if (_monitor.TryGetRamUsageGb(out var used, out var ramUsedReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", used))
                            {
                                ramUsed.Add(used);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Data, "RAM Used GB", ramUsedReason);
                        }

                        if (_monitor.TryGetRamLoad(out var load, out var ramLoadReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Load, "RAM Load %", load))
                            {
                                ramLoads.Add(load);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Ram, SensorType.Load, "RAM Load %", ramLoadReason);
                        }
                    }

                    if (options.ScanDisk)
                    {
                        if (_monitor.TryGetDiskLoad(out var diskLoad, out var diskLoadReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Load, "Disk Load %", diskLoad))
                            {
                                diskLoads.Add(diskLoad);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Load, "Disk Load %", diskLoadReason);
                        }

                        if (_monitor.TryGetDiskReadRateMbit(out var readRate, out var diskReadReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read Mb/s", readRate))
                            {
                                diskReads.Add(readRate);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Throughput, "Disk Read Mb/s", diskReadReason);
                        }

                        if (_monitor.TryGetDiskWriteRateMbit(out var writeRate, out var diskWriteReason))
                        {
                            if (AddReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write Mb/s", writeRate))
                            {
                                diskWrites.Add(writeRate);
                            }
                        }
                        else
                        {
                            AddUnavailableReading(result.UnderLoadReadings, ComponentType.Disk, SensorType.Throughput, "Disk Write Mb/s", diskWriteReason);
                        }
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
            float maxPossibleDeduction = 0f;

            if (options.ScanCPU)
            {
                if (cpuTemps.Any())
                {
                    score -= Math.Min(3f, Math.Max(0, result.PeakCpuTemp / 100f * 3f));
                    maxPossibleDeduction += 3f;
                }

                if (cpuLoads.Any())
                {
                    score -= Math.Min(2f, Math.Max(0, result.AvgCpuLoad / 100f * 2f));
                    maxPossibleDeduction += 2f;
                }
            }

            if (options.ScanGPU)
            {
                if (gpuTemps.Any())
                {
                    score -= Math.Min(2f, Math.Max(0, result.PeakGpuTemp / 100f * 2f));
                    maxPossibleDeduction += 2f;
                }

                if (gpuLoads.Any())
                {
                    score -= Math.Min(1.5f, Math.Max(0, gpuLoads.Average() / 100f * 1.5f));
                    maxPossibleDeduction += 1.5f;
                }
            }

            if (options.ScanRAM)
            {
                if (ramLoads.Any())
                {
                    score -= Math.Min(2f, Math.Max(0, result.AvgRamLoad / 100f * 2f));
                    maxPossibleDeduction += 2f;
                }
            }

            if (options.ScanDisk)
            {
                if (diskLoads.Any())
                {
                    score -= Math.Min(1.5f, Math.Max(0, result.AvgDiskLoad / 100f * 1.5f));
                    maxPossibleDeduction += 1.5f;
                }
            }

            // Normalizar score solo con métricas efectivamente disponibles.
            if (maxPossibleDeduction > 0)
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
                CpuLoad = cpuLoads.Any() ? result.AvgCpuLoad : 0f,
                CpuClock = cpuClocks.Any() ? cpuClocks.Average() : 0f,
                GpuTemp = gpuTemps.Any() ? gpuTemps.Average() : 0f,
                GpuLoad = gpuLoads.Any() ? gpuLoads.Average() : 0f,
                RamUsed = ramUsed.Any() ? ramUsed.Average() : 0f,
                RamLoad = ramLoads.Any() ? result.AvgRamLoad : 0f,
                DiskLoad = diskLoads.Any() ? result.AvgDiskLoad : 0f,
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