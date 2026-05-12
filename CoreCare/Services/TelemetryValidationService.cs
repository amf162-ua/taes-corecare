using System;
using CoreCare.Models;

namespace CoreCare.Services
{
    public class TelemetryValidationService
    {
        public bool TryValidate(ComponentType component, SensorType type, string metricName, float value, out string reason)
        {
            reason = string.Empty;

            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                reason = "Value is not finite.";
                return false;
            }

            if (value < 0f)
            {
                reason = "Value cannot be negative.";
                return false;
            }

            if (!IsSupportedCombination(component, type))
            {
                reason = $"Unsupported metric type {type} for component {component}.";
                return false;
            }

            var expectedUnit = GetExpectedUnit(component, type);
            if (!metricName.Contains(expectedUnit, StringComparison.OrdinalIgnoreCase))
            {
                reason = $"Unit mismatch. Expected unit '{expectedUnit}' in metric name '{metricName}'.";
                return false;
            }

            if (!IsWithinRange(component, type, value, out var rangeReason))
            {
                reason = rangeReason;
                return false;
            }

            return true;
        }

        private static bool IsSupportedCombination(ComponentType component, SensorType type)
        {
            return component switch
            {
                ComponentType.Cpu => type == SensorType.Temperature || type == SensorType.Load || type == SensorType.Clock,
                ComponentType.Gpu => type == SensorType.Temperature || type == SensorType.Load,
                ComponentType.Ram => type == SensorType.Data || type == SensorType.Load,
                ComponentType.Disk => type == SensorType.Load || type == SensorType.Throughput,
                _ => false
            };
        }

        private static string GetExpectedUnit(ComponentType component, SensorType type)
        {
            return (component, type) switch
            {
                (_, SensorType.Temperature) => "C",
                (_, SensorType.Load) => "%",
                (_, SensorType.Clock) => "GHz",
                (ComponentType.Ram, SensorType.Data) => "GB",
                (ComponentType.Disk, SensorType.Throughput) => "Mb/s",
                _ => string.Empty
            };
        }

        private static bool IsWithinRange(ComponentType component, SensorType type, float value, out string reason)
        {
            reason = string.Empty;

            switch (component, type)
            {
                case (_, SensorType.Load):
                    if (value > 100f)
                    {
                        reason = "Load must be in range 0-100%.";
                        return false;
                    }
                    return true;

                case (_, SensorType.Temperature):
                    if (value > 125f)
                    {
                        reason = "Temperature is outside expected range (0-125 C).";
                        return false;
                    }
                    return true;

                case (ComponentType.Cpu, SensorType.Clock):
                    if (value is < 0.1f or > 10f)
                    {
                        reason = "CPU clock is outside expected range (0.1-10 GHz).";
                        return false;
                    }
                    return true;

                case (ComponentType.Ram, SensorType.Data):
                    if (value > 2048f)
                    {
                        reason = "RAM used is outside expected range (0-2048 GB).";
                        return false;
                    }
                    return true;

                case (ComponentType.Disk, SensorType.Throughput):
                    if (value > 200_000f)
                    {
                        reason = "Disk throughput is outside expected range (0-200000 Mb/s).";
                        return false;
                    }
                    return true;

                default:
                    return true;
            }
        }
    }
}
