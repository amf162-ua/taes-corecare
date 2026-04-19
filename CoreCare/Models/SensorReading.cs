using System;
using System.Collections.Generic;
using System.Text;

// Unidad mínima de datos: representa una lectura puntual
// de un sensor de un componente concreto.
namespace CoreCare.Models
{
    public enum SensorType
    {
        Temperature,
        Load,
        Clock,
        Voltage,
        Fan,
        Power,
        Data,
        Throughput,
    }

    public enum ComponentType
    {
        Cpu,
        Gpu,
        Ram,
        Disk
    }
    public class SensorReading
    {
        public int Id { get; set; }
        public ComponentType Component { get; set; }
        public SensorType Type { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime TimeStamp { get; set; }

        // Referencia a RegistroBenchmark (opcional, solo para lecturas persistidas)
        public int? RegistroBenchmarkId { get; set; }
        public RegistroBenchmark? RegistroBenchmark { get; set; }
    }
}
