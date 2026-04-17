using CoreCare.Models;
using System;
using System.Collections.Generic;

namespace CoreCare.Models
{
    public class ReportData
    {
        public string CompanyName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }

        // Datos del sistema analizado (reutilizamos el que creaste con tu compañero)
        public SystemTelemetryMock TelemetryData { get; set; } = new SystemTelemetryMock();

        // Recomendaciones (hasta que tu compañero haga el Parser, lo simularemos con strings)
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}
