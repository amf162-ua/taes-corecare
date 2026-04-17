using System;
using System.IO;
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

                column.Item().Text("Resultados del Benchmark (Telemetría)").FontSize(14).SemiBold();
                column.Item().Element(ComposeTable);

                column.Item().Text("Recomendaciones de IA para el Cliente").FontSize(14).SemiBold();
                
                if (_data.Recommendations != null && _data.Recommendations.Count > 0)
                {
                    foreach (var rec in _data.Recommendations)
                    {
                        column.Item().Row(row =>
                        {
                            row.Spacing(5);
                            row.AutoItem().Text("•");
                            row.RelativeItem().Text(rec);
                        });
                    }
                }
                else
                {
                    column.Item().Text("No se generaron recomendaciones para este dispositivo.").Italic().FontColor(Colors.Grey.Medium);
                }
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

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Uso GPU (%)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.DiskUsagePercent:F1}%");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Temperatura GPU (°C)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.CpuTemperatureC:F1}°C");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("RAM Utilizada / Total");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.RamUsedGb:F1}GB / {_data.TelemetryData.RamTotalGb:F1}GB");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Tipo de Disco");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_data.TelemetryData.DiskType}");
            });
        }

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