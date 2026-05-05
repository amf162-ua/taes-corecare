using System;
using System.Collections.Generic;
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
        private const string Primary = "#01696F";
        private const string Accent = "#FF6100";
        private const string Ink = "#1F2933";
        private const string Muted = "#697386";
        private const string Panel = "#F6F8FA";
        private const string Border = "#DDE3EA";

        public ReportDocument(ReportData data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(38);
                page.MarginVertical(32);
                page.DefaultTextStyle(TextStyle.Default.FontSize(9).FontColor(Ink));

                page.Header().Element(ComposeHeader);
                page.Content().PaddingTop(18).Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.BorderBottom(1).BorderColor(Border).PaddingBottom(14).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("CoreCare").FontSize(11).SemiBold().FontColor(Primary);
                    column.Item().Text("Informe técnico de sistema").FontSize(24).Bold().FontColor(Ink);
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Cliente: ").SemiBold();
                        text.Span(_data.ClientName);
                        text.Span("   |   ");
                        text.Span("Fecha: ").SemiBold();
                        text.Span(_data.ReportDate.ToString("dd/MM/yyyy HH:mm"));
                    });
                });

                row.ConstantItem(120).AlignRight().Element(ComposeLogo);
            });
        }

        private void ComposeLogo(IContainer container)
        {
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Logo_CoreCare.png");
                if (File.Exists(logoPath))
                {
                    container.Height(58).Image(logoPath).FitArea();
                    return;
                }
            }
            catch
            {
            }

            container.Height(58)
                .Border(1)
                .BorderColor(Border)
                .Background(Panel)
                .AlignCenter()
                .AlignMiddle()
                .Text("CoreCare")
                .FontColor(Primary)
                .SemiBold();
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(16);

                column.Item().Element(ComposeMetricCards);
                column.Item().Element(c => ComposeSection(c, "Especificaciones del sistema", ComposeSpecs));
                column.Item().Element(c => ComposeSection(c, "Telemetría actual", ComposeTelemetryTable));
                column.Item().Element(c => ComposeSection(c, "Recomendaciones", ComposeRecommendations));

                if (_data.TelemetryWarnings.Count > 0)
                {
                    column.Item().Element(c => ComposeSection(c, "Avisos de telemetría", ComposeWarnings));
                }
            });
        }

        private void ComposeMetricCards(IContainer container)
        {
            container.Row(row =>
            {
                row.Spacing(8);
                row.RelativeItem().Element(c => ComposeMetricCard(c, "CPU", FormatPercent(_data.TelemetryData.CpuUsagePercent), FormatMetric(_data.TelemetryData.CpuTemperatureC, "C")));
                row.RelativeItem().Element(c => ComposeMetricCard(c, "GPU", FormatPercent(_data.TelemetryData.GpuUsagePercent), FormatMetric(_data.TelemetryData.GpuTemperatureC, "C")));
                row.RelativeItem().Element(c => ComposeMetricCard(c, "RAM", $"{_data.TelemetryData.RamUsedGb:F1} GB", $"{_data.TelemetryData.RamTotalGb:F1} GB total"));
                row.RelativeItem().Element(c => ComposeMetricCard(c, "Disco", FormatMetric(_data.TelemetryData.DiskUsagePercent, "%"), _data.TelemetryData.DiskType));
            });
        }

        private static void ComposeMetricCard(IContainer container, string title, string value, string detail)
        {
            container.Border(1).BorderColor(Border).Background(Panel).Padding(10).Column(column =>
            {
                column.Item().Text(title.ToUpperInvariant()).FontSize(8).SemiBold().FontColor(Muted);
                column.Item().PaddingTop(4).Text(value).FontSize(17).Bold().FontColor(Primary);
                column.Item().PaddingTop(2).Text(detail).FontSize(8).FontColor(Muted);
            });
        }

        private static void ComposeSection(IContainer container, string title, Action<IContainer> content)
        {
            container.Column(column =>
            {
                column.Spacing(7);
                column.Item().Row(row =>
                {
                    row.ConstantItem(4).Height(14).Background(Accent);
                    row.RelativeItem().PaddingLeft(7).Text(title).FontSize(13).SemiBold().FontColor(Ink);
                });
                column.Item().Element(content);
            });
        }

        private void ComposeSpecs(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.1f);
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(1.1f);
                    columns.RelativeColumn(2.2f);
                });

                AddSpecRow(table, "Procesador", _data.SystemSpecs.CpuName, "Núcleos / hilos", $"{_data.SystemSpecs.CpuCores} / {_data.SystemSpecs.CpuThreads}");
                AddSpecRow(table, "Frecuencia máx.", _data.SystemSpecs.CpuMaxClockSpeed, "Caché L2 / L3", $"{_data.SystemSpecs.CpuCacheL2} / {_data.SystemSpecs.CpuCacheL3}");
                AddSpecRow(table, "Gráfica", _data.SystemSpecs.GpuName, "VRAM", _data.SystemSpecs.GpuVram);
                AddSpecRow(table, "Driver GPU", _data.SystemSpecs.GpuDriverVersion, "Almacenamiento", $"{_data.SystemSpecs.DiskModel} ({_data.SystemSpecs.DiskSize})");
            });
        }

        private static void AddSpecRow(TableDescriptor table, string labelA, string valueA, string labelB, string valueB)
        {
            AddLabelCell(table, labelA);
            AddValueCell(table, valueA);
            AddLabelCell(table, labelB);
            AddValueCell(table, valueB);
        }

        private void ComposeTelemetryTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                AddTelemetryRow(table, "Uso CPU", FormatPercent(_data.TelemetryData.CpuUsagePercent), "Temperatura CPU", FormatMetric(_data.TelemetryData.CpuTemperatureC, "C"));
                AddTelemetryRow(table, "Uso GPU", FormatPercent(_data.TelemetryData.GpuUsagePercent), "Temperatura GPU", FormatMetric(_data.TelemetryData.GpuTemperatureC, "C"));
                AddTelemetryRow(table, "RAM", $"{_data.TelemetryData.RamUsedGb:F1} / {_data.TelemetryData.RamTotalGb:F1} GB", "Uso de disco", FormatMetric(_data.TelemetryData.DiskUsagePercent, "%"));
            });
        }

        private static void AddTelemetryRow(TableDescriptor table, string labelA, string valueA, string labelB, string valueB)
        {
            AddLabelCell(table, labelA);
            AddValueCell(table, valueA);
            AddLabelCell(table, labelB);
            AddValueCell(table, valueB);
        }

        private void ComposeRecommendations(IContainer container)
        {
            var cards = ParseRecommendationCards(_data.Recommendations);

            container.Column(column =>
            {
                column.Spacing(8);

                if (_data.RecommendationsGeneratedLocally)
                {
                    column.Item().Background("#FFF7ED").Border(1).BorderColor("#FDBA74").Padding(8)
                        .Text("Recomendaciones generadas localmente porque la IA no respondió a tiempo.")
                        .FontSize(8)
                        .FontColor("#9A3412");
                }

                if (cards.Count == 0)
                {
                    column.Item().Text("No se generaron recomendaciones para este dispositivo.").Italic().FontColor(Muted);
                    return;
                }

                foreach (var card in cards)
                {
                    column.Item().Element(c => ComposeRecommendationCard(c, card));
                }
            });
        }

        private static void ComposeRecommendationCard(IContainer container, RecommendationCard card)
        {
            var color = card.Priority.Contains("alta", StringComparison.OrdinalIgnoreCase)
                ? "#B42318"
                : card.Priority.Contains("media", StringComparison.OrdinalIgnoreCase)
                    ? "#B54708"
                    : Primary;

            container.Border(1).BorderColor(Border).Background("#FFFFFF").Row(row =>
            {
                row.ConstantItem(5).Background(color);
                row.RelativeItem().Padding(10).Column(column =>
                {
                    column.Spacing(5);
                    column.Item().Text(card.Priority).FontSize(8).SemiBold().FontColor(color);
                    column.Item().Text(card.Title).FontSize(11).SemiBold().FontColor(Ink);

                    if (!string.IsNullOrWhiteSpace(card.Problem))
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Problema: ").SemiBold();
                            text.Span(card.Problem);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(card.Solution))
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Solución: ").SemiBold();
                            text.Span(card.Solution);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(card.Cost) || !string.IsNullOrWhiteSpace(card.Impact))
                    {
                        column.Item().PaddingTop(3).Text(text =>
                        {
                            if (!string.IsNullOrWhiteSpace(card.Cost))
                            {
                                text.Span("Coste: ").SemiBold();
                                text.Span(card.Cost);
                            }

                            if (!string.IsNullOrWhiteSpace(card.Cost) && !string.IsNullOrWhiteSpace(card.Impact))
                            {
                                text.Span("   ");
                            }

                            if (!string.IsNullOrWhiteSpace(card.Impact))
                            {
                                text.Span("Impacto: ").SemiBold();
                                text.Span(card.Impact);
                            }
                        });
                    }
                });
            });
        }

        private void ComposeWarnings(IContainer container)
        {
            container.Background("#FFFBEB").Border(1).BorderColor("#FCD34D").Padding(9).Column(column =>
            {
                column.Spacing(3);
                foreach (var warning in _data.TelemetryWarnings.Where(w => !string.IsNullOrWhiteSpace(w)))
                {
                    column.Item().Text("- " + warning).FontSize(8).FontColor("#92400E");
                }
            });
        }

        private static void AddLabelCell(TableDescriptor table, string label)
        {
            table.Cell().BorderBottom(1).BorderColor(Border).Background(Panel).Padding(6).Text(label).SemiBold().FontColor(Muted);
        }

        private static void AddValueCell(TableDescriptor table, string value)
        {
            table.Cell().BorderBottom(1).BorderColor(Border).Padding(6).Text(string.IsNullOrWhiteSpace(value) ? "N/A" : value).FontColor(Ink);
        }

        private static string FormatPercent(double value)
            => value < 0 ? "No disponible" : $"{value:F1}%";

        private static string FormatMetric(double value, string suffix)
            => value < 0 ? "No disponible" : $"{value:F1} {suffix}";

        private static List<RecommendationCard> ParseRecommendationCards(IEnumerable<string> recommendations)
        {
            var cards = new List<RecommendationCard>();

            foreach (var recommendation in recommendations.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                var lines = recommendation
                    .Replace("\r", string.Empty)
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                string priority = "Recomendación";
                RecommendationCard? current = null;

                foreach (var line in lines)
                {
                    if (line.StartsWith("===", StringComparison.Ordinal))
                    {
                        priority = line.Trim('=').Trim();
                        continue;
                    }

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        if (current is not null)
                        {
                            cards.Add(current);
                        }

                        current = new RecommendationCard(priority, line.Trim('[', ']'), string.Empty, string.Empty, string.Empty, string.Empty);
                        continue;
                    }

                    current ??= new RecommendationCard(priority, "Recomendación general", string.Empty, string.Empty, string.Empty, string.Empty);

                    if (line.StartsWith("- Problema:", StringComparison.OrdinalIgnoreCase))
                    {
                        current = current with { Problem = CleanValue(line, "- Problema:") };
                    }
                    else if (line.StartsWith("- Solución:", StringComparison.OrdinalIgnoreCase) || line.StartsWith("- Solucion:", StringComparison.OrdinalIgnoreCase))
                    {
                        current = current with { Solution = CleanValue(line, line.Contains("Solución", StringComparison.OrdinalIgnoreCase) ? "- Solución:" : "- Solucion:") };
                    }
                    else if (line.StartsWith("- Coste:", StringComparison.OrdinalIgnoreCase))
                    {
                        current = current with { Cost = CleanValue(line, "- Coste:") };
                    }
                    else if (line.StartsWith("- Impacto:", StringComparison.OrdinalIgnoreCase))
                    {
                        current = current with { Impact = CleanValue(line, "- Impacto:") };
                    }
                    else if (string.IsNullOrWhiteSpace(current.Problem))
                    {
                        current = current with { Problem = line.TrimStart('-', ' ') };
                    }
                    else
                    {
                        current = current with { Solution = AppendText(current.Solution, line.TrimStart('-', ' ')) };
                    }
                }

                if (current is not null)
                {
                    cards.Add(current);
                }
            }

            return cards;
        }

        private static string CleanValue(string line, string prefix)
            => line.Length <= prefix.Length ? string.Empty : line[prefix.Length..].Trim();

        private static string AppendText(string current, string next)
            => string.IsNullOrWhiteSpace(current) ? next : current + " " + next;

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Border).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text(_data.CompanyName).FontSize(8).FontColor(Muted);
                row.ConstantItem(120).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Muted));
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }

        private sealed record RecommendationCard(
            string Priority,
            string Title,
            string Problem,
            string Solution,
            string Cost,
            string Impact);
    }
}
