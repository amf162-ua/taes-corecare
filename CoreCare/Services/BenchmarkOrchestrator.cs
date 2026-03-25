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

            //LECTURA 
            for (int i = 0; i < 3; i++)
            {
                _monitor.UpdateHardware();
                var cpuTempData = _monitor.GetCpuTemperature();

                result.BaselineReadings.Add(new SensorReading
                {
                    Component = ComponentType.Cpu,
                    Type = SensorType.Temperature,
                    Name = "CPU Core",
                    Value = cpuTempData.Value,
                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(cpuTempData.Timestamp).DateTime
                });

                await Task.Delay(1000);
            }

            //ESTRÉS
            if (options.ScanCPU) _stressWorker.RunCpuStress(durationSeconds);
            if (options.ScanDisk) _stressWorker.RunDiskStress(durationSeconds);
            if (options.ScanRAM) _stressWorker.RunRamStress(durationSeconds);

            DateTime endTime = DateTime.Now.AddSeconds(durationSeconds);

            while (DateTime.Now < endTime)
            {
                _monitor.UpdateHardware();
                var cpuTempData = _monitor.GetCpuTemperature();
                float cpuLoad = _monitor.GetCpuLoad();

                result.UnderLoadReadings.Add(new SensorReading
                {
                    Component = ComponentType.Cpu,
                    Type = SensorType.Temperature,
                    Name = "CPU Core",
                    Value = cpuTempData.Value,
                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(cpuTempData.Timestamp).DateTime
                });

                await Task.Delay(1000);
            }

            // CÁLCULO DE RESULTADOS
            result.PeakCpuTemp = (float)result.UnderLoadReadings.Max(r => r.Value);
            result.AvgCpuLoad = _monitor.GetCpuLoad();
            float avgTemp = (float)result.UnderLoadReadings.Average(r => r.Value);

            result.Score = 10000 - (result.PeakCpuTemp * 20);

            // Construcción del registro final para la base de datos
            var registroFinal = new RegistroBenchmark
            {
                Timestamp = DateTime.Now,
                CpuTemp = avgTemp,
                CpuLoad = result.AvgCpuLoad,
                Score = result.Score
            };

            return registroFinal;
        }
    }
}