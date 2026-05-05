using System;
using System.IO;
using System.Linq;
using CoreCare.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoreCare.Services
{
    public class ReportDocument : IDocument
    {
        private readonly ReportData _data;

        public ReportDocument(ReportData data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INFORME DE AUDITORÍA").Style(titleStyle);
                    column.Item().Text(text =>
                    {
                        text.Span("Emitido por: ").SemiBold();
                        text.Span(_data.CompanyName);
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Cliente: ").SemiBold();
                        text.Span(_data.ClientName);
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Fecha: ").SemiBold();
                        text.Span(_data.ReportDate.ToString("dd/MM/yyyy"));
                    });
                });

                // Espacio para insertar el logotipo si existe
                try
                {
                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Logo_CoreCare.png");
                    if (File.Exists(logoPath))
                    {
                        row.ConstantItem(150).Height(75).Image(logoPath);
                    }
                    else
                    {
                        row.ConstantItem(150).Height(75).Placeholder(); // Fallback si no se encontró
                    }
                }
                catch
                {
                    row.ConstantItem(150).Height(75).Placeholder();
                }
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().Text("Especificaciones del Sistema").FontSize(14).SemiBold();
                column.Item().Element(ComposeSpecs);

                column.Item().Text("Resultados del Benchmark (Telemetría)").FontSize(14).SemiBold();
                column.Item().Element(ComposeTable);

                column.Item().Text("Recomendaciones de IA para el Cliente").FontSize(14).SemiBold();

                if (_data.Recommendations != null && _data.Recommendations.Count > 0)
                {
                    foreach (var recommendation in _data.Recommendations.Where(r => !string.IsNullOrWhiteSpace(r)))
                    {
                        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(10).Text(text =>
                        {
                            text.Span(recommendation).FontSize(9);
                        });
                    }
                }
                else
                {
                    column.Item().Text("No se generaron recomendaciones para este dispositivo.").Italic().FontColor(Colors.Grey.Medium);
                }
            });
        }

        private void ComposeSpecs(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Componente").FontColor(Colors.White).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Especificación").FontColor(Colors.White).SemiBold();
                });

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Procesador");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_data.SystemSpecs.CpuName);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Núcleos / Hilos");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.SystemSpecs.CpuCores} núcleos / {_data.SystemSpecs.CpuThreads} hilos");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Frecuencia máx.");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_data.SystemSpecs.CpuMaxClockSpeed);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Caché L2 / L3");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.SystemSpecs.CpuCacheL2} / {_data.SystemSpecs.CpuCacheL3}");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Gráfica");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_data.SystemSpecs.GpuName);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("VRAM");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_data.SystemSpecs.GpuVram);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Driver GPU");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_data.SystemSpecs.GpuDriverVersion);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("RAM Total");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.RamTotalGb:F1} GB");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Almacenamiento");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.SystemSpecs.DiskModel} ({_data.SystemSpecs.DiskSize})");
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // Configurar columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // Estilo para las cabeceras de tabla
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Métrica").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Valor leído").SemiBold();
                });

                // Filas de datos usando TelemetryMock
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Uso CPU (%)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.CpuUsagePercent:F1}%");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Temperatura CPU (°C)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatMetric(_data.TelemetryData.CpuTemperatureC, "°C"));

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Uso GPU (%)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.GpuUsagePercent:F1}%");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Temperatura GPU (°C)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.GpuTemperatureC:F1}°C");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("RAM Utilizada / Total");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.RamUsedGb:F1}GB / {_data.TelemetryData.RamTotalGb:F1}GB");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Tipo de Disco");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.DiskType}");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Uso de Disco (%)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatMetric(_data.TelemetryData.DiskUsagePercent, "%"));
            });
        }

        private static string FormatMetric(double value, string suffix)
            => value < 0 ? "No disponible" : $"{value:F1}{suffix}";

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        }
    }
}
