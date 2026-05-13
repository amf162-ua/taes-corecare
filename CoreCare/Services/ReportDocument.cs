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
        private const string Primary = "#0F766E";
        private const string Ink = "#111827";
        private const string Muted = "#6B7280";
        private const string Surface = "#F8FAFC";
        private const string HeaderSurface = "#F3F4F6";
        private const string Border = "#E5E7EB";
        private const string Warning = "#B45309";
        private const string Critical = "#B91C1C";

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
                page.Content().PaddingTop(16).Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.BorderBottom(1).BorderColor(Border).PaddingBottom(12).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Spacing(3);
                    column.Item().Text("CoreCare").FontSize(10).SemiBold().FontColor(Primary);
                    column.Item().Text("Informe técnico del sistema").FontSize(22).SemiBold().FontColor(Ink);
                    column.Item().Text(text =>
                    {
                        text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Muted));
                        text.Span("Cliente: ").SemiBold();
                        text.Span(_data.ClientName);
                        text.Span("  /  ");
                        text.Span("Empresa: ").SemiBold();
                        text.Span(_data.CompanyName);
                        text.Span("  /  ");
                        text.Span("Fecha: ").SemiBold();
                        text.Span(_data.ReportDate.ToString("dd/MM/yyyy HH:mm"));
                    });
                });

                row.ConstantItem(96).AlignRight().Element(ComposeLogo);
            });
        }

        private void ComposeLogo(IContainer container)
        {
            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Logo_CoreCare.png");
                if (File.Exists(logoPath))
                {
                    container.Height(44).Image(logoPath).FitArea();
                    return;
                }
            }
            catch
            {
            }

            container.Height(44)
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
                column.Spacing(15);

                column.Item().Element(c => ComposeSection(c, "Especificaciones del sistema", ComposeSpecs));
                column.Item().Element(c => ComposeSection(c, "Telemetría actual", ComposeTelemetryTable));
                if (_data.BenchmarkResults.Count > 0)
                {
                    column.Item().Element(c => ComposeSection(c, "Benchmarks ejecutados", ComposeBenchmarkResults));
                }

                column.Item().Element(c => ComposeSection(c, "Recomendaciones de IA", ComposeRecommendations));
                column.Item().Element(c => ComposeSection(c, "Mejoras de hardware sugeridas", ComposeUpgradeAdvice));

                if (_data.Sponsors != null && _data.Sponsors.Count > 0)
                {
                    column.Item().Element(c => ComposeSection(c, "Patrocinadores", ComposeSponsors));
                }
            });
        }

        private void ComposeSponsors(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                foreach (var sponsor in _data.Sponsors)
                {
                    column.Item().Background(Surface).Border(1).BorderColor(Border).Padding(10).Column(c =>
                    {
                        c.Spacing(4);
                        c.Item().Text(sponsor.Name).FontSize(11).SemiBold().FontColor(Primary);
                        c.Item().Text(sponsor.Message).FontSize(9).FontColor(Ink);
                        if (!string.IsNullOrWhiteSpace(sponsor.Website))
                        {
                            c.Item().Text(sponsor.Website).FontSize(8).FontColor(Muted).Underline();
                        }
                    });
                }
            });
        }

        private static void ComposeSection(IContainer container, string title, Action<IContainer> content)
        {
            container.Column(column =>
            {
                column.Spacing(8);
                column.Item().BorderBottom(1).BorderColor(Border).PaddingBottom(4)
                    .Text(title).FontSize(12).SemiBold().FontColor(Ink);
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

        private void ComposeBenchmarkResults(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                foreach (var benchmark in _data.BenchmarkResults.OrderBy(result => result.Timestamp))
                {
                    column.Item().Background(Surface).Border(1).BorderColor(Border).Padding(10).Column(card =>
                    {
                        card.Spacing(8);
                        card.Item().Row(row =>
                        {
                            row.RelativeItem().Column(header =>
                            {
                                header.Spacing(2);
                                header.Item().Text(benchmark.BenchmarkTypeLabel).FontSize(11).SemiBold().FontColor(Ink);
                                header.Item().Text(benchmark.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(8).FontColor(Muted);
                            });

                            row.AutoItem().Text($"{benchmark.Score:F1}/10")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor(ScoreColor(benchmark.Score));
                        });

                        card.Item().Element(c => ComposeBenchmarkMetricTable(c, benchmark));

                        var unavailableReadings = benchmark.SensorReadings
                            .Where(reading => reading.Name.Contains("NO DISPONIBLE", StringComparison.OrdinalIgnoreCase))
                            .Select(reading => reading.Name)
                            .Distinct()
                            .ToList();

                        foreach (var unavailable in unavailableReadings)
                        {
                            card.Item().Element(c => ComposeTechnicalNote(c, $"Sensor no disponible durante el benchmark: {unavailable}"));
                        }
                    });
                }
            });
        }

        private static void ComposeBenchmarkMetricTable(IContainer container, RegistroBenchmark benchmark)
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

                table.Header(header =>
                {
                    AddHeaderCell(header, "Componente");
                    AddHeaderCell(header, "Métrica 1");
                    AddHeaderCell(header, "Métrica 2");
                    AddHeaderCell(header, "Métrica 3");
                });

                var components = benchmark.ComponentsScanned;
                if (components.Contains(ComponentType.Cpu))
                {
                    AddBenchmarkMetricRow(table, "CPU", FormatPercent(benchmark.CpuLoad), FormatMetric(benchmark.CpuTemp, "C"), $"{benchmark.CpuClock:F2} GHz");
                }

                if (components.Contains(ComponentType.Gpu))
                {
                    AddBenchmarkMetricRow(table, "GPU", FormatPercent(benchmark.GpuLoad), FormatMetric(benchmark.GpuTemp, "C"), "N/A");
                }

                if (components.Contains(ComponentType.Ram))
                {
                    AddBenchmarkMetricRow(table, "RAM", FormatMetric(benchmark.RamUsed, "GB"), FormatPercent(benchmark.RamLoad), "N/A");
                }

                if (components.Contains(ComponentType.Disk))
                {
                    AddBenchmarkMetricRow(table, "Disco", FormatPercent(benchmark.DiskLoad), $"{benchmark.DiskReadRate:F2} Mb/s lectura", $"{benchmark.DiskWriteRate:F2} Mb/s escritura");
                }

                if (components.Count == 0)
                {
                    AddBenchmarkMetricRow(table, "Benchmark", "Sin sensores asociados", "N/A", "N/A");
                }
            });
        }

        private static void AddBenchmarkMetricRow(TableDescriptor table, string component, string metricA, string metricB, string metricC)
        {
            AddLabelCell(table, component);
            AddValueCell(table, metricA);
            AddValueCell(table, metricB);
            AddValueCell(table, metricC);
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
                    column.Item().Element(c => ComposeTechnicalNote(c, cards.Count == 0
                        ? "No se pudieron generar recomendaciones de IA por un error del servicio."
                        : "No se pudieron generar recomendaciones de IA por un error del servicio. Se muestran avisos locales basados en la telemetría disponible."));
                }

                if (cards.Count == 0)
                {
                    column.Item().Text(_data.RecommendationsGeneratedLocally
                        ? "No hay recomendaciones locales relevantes con la telemetría disponible."
                        : "No se generaron recomendaciones de IA para este dispositivo.")
                        .Italic()
                        .FontColor(Muted);
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
            var color = PriorityColor(card.Priority);

            container.PaddingVertical(6).BorderBottom(1).BorderColor(Border).Column(column =>
            {
                column.Spacing(5);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(card.Title).FontSize(10.5f).SemiBold().FontColor(Ink);
                    row.AutoItem().Element(c => ComposeStatusBadge(c, card.Priority, color));
                });

                if (!string.IsNullOrWhiteSpace(card.Problem))
                {
                    column.Item().Text(text =>
                    {
                        text.Span("Problema: ").SemiBold().FontColor(Muted);
                        text.Span(card.Problem).FontColor(Ink);
                    });
                }

                if (!string.IsNullOrWhiteSpace(card.Solution))
                {
                    column.Item().Text(text =>
                    {
                        text.Span("Solución: ").SemiBold().FontColor(Muted);
                        text.Span(card.Solution).FontColor(Ink);
                    });
                }

            });
        }

        private void ComposeWarnings(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(5);
                foreach (var warning in _data.TelemetryWarnings.Where(w => !string.IsNullOrWhiteSpace(w)))
                {
                    column.Item().Element(c => ComposeTechnicalNote(c, warning));
                }
            });
        }

        private static void ComposeTechnicalNote(IContainer container, string message)
        {
            container.Background(Surface).Padding(8).Row(row =>
            {
                row.ConstantItem(2).Background(Border);
                row.RelativeItem().PaddingLeft(8).Text(message).FontSize(8).FontColor(Muted);
            });
        }

        private void ComposeUpgradeAdvice(IContainer container)
        {
            var profiles = new[] { UpgradeProfile.General, UpgradeProfile.Gaming, UpgradeProfile.HeavyWork, UpgradeProfile.VeryHeavyWork };

            container.Column(column =>
            {
                column.Spacing(12);

                foreach (var profile in profiles)
                {
                    column.Item().Element(c => ComposeUpgradeProfileBlock(c, profile));
                }
            });
        }

        private void ComposeUpgradeProfileBlock(IContainer container, UpgradeProfile profile)
        {
            container.PaddingTop(4).Column(column =>
            {
                column.Spacing(8);

                column.Item().Row(row =>
                {
                    row.ConstantItem(3).Background(Primary);
                    row.RelativeItem().PaddingLeft(8).Column(header =>
                    {
                        header.Spacing(2);
                        header.Item().Text(ProfileLabel(profile)).FontSize(11.5f).SemiBold().FontColor(Ink);
                        header.Item().Text(ProfileDescription(profile)).FontSize(8).FontColor(Muted);
                        header.Item().Text("La puntuación compara el equipo actual con una referencia razonable para este tipo de uso. Por debajo de 6,5 se considera recomendable valorar una mejora.").FontSize(7.5f).FontColor(Muted);
                    });
                });

                column.Item().Element(c => ComposeProfileScoreTable(c, profile));
                column.Item().Element(c => ComposeProfileRecommendations(c, profile));
            });
        }

        private void ComposeProfileScoreTable(IContainer container, UpgradeProfile profile)
        {
            var components = new[] { "CPU", "RAM", "GPU", "Disco" };

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn(3.2f);
                });

                table.Header(header =>
                {
                    AddHeaderCell(header, "Componente");
                    AddHeaderCell(header, "Puntuación");
                    AddHeaderCell(header, "Lectura");
                });

                foreach (var component in components)
                {
                    var score = _data.UpgradeScores.FirstOrDefault(item => item.Component == component && item.Profile == profile);
                    AddLabelCell(table, component);
                    AddScoreCell(table, score?.Score);
                    AddValueCell(table, score?.Reason ?? "Sin datos suficientes para puntuar este componente.");
                }
            });
        }

        private void ComposeProfileRecommendations(IContainer container, UpgradeProfile profile)
        {
            var recommendations = _data.UpgradeRecommendations
                .Where(recommendation => recommendation.Profile == profile)
                .OrderBy(recommendation => recommendation.CurrentScore)
                .ThenBy(recommendation => recommendation.Component)
                .ToList();

            container.Column(column =>
            {
                column.Spacing(7);
                column.Item().Text("Reemplazos sugeridos para este caso").FontSize(9).SemiBold().FontColor(Ink);

                if (recommendations.Count == 0)
                {
                    column.Item().Background(Surface).Padding(8)
                        .Text("No hay reemplazos prioritarios para esta categoría. El equipo tiene margen suficiente para este tipo de uso, aunque puede seguir beneficiándose de mantenimiento, limpieza y actualizaciones de software.")
                        .FontSize(8)
                        .FontColor(Muted);
                    return;
                }

                foreach (var recommendation in recommendations)
                {
                    column.Item().Element(c => ComposeUpgradeCard(c, recommendation));
                }
            });
        }

        private static void ComposeUpgradeCard(IContainer container, UpgradeRecommendation recommendation)
        {
            var color = PriorityColor(recommendation.Priority);

            container.PaddingVertical(6).BorderBottom(1).BorderColor(Border).Column(column =>
            {
                column.Spacing(5);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"{recommendation.Component}: {recommendation.SuggestedUpgrade}").FontSize(10).SemiBold().FontColor(Ink);
                    row.AutoItem().Element(c => ComposeStatusBadge(c, recommendation.Priority, color));
                });

                column.Item().Text(text =>
                {
                    text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Muted));
                    text.Span("Componente actual: ").SemiBold();
                    text.Span(recommendation.CurrentComponent);
                    text.Span("   ");
                    text.Span("Puntuación: ").SemiBold();
                    text.Span($"{recommendation.CurrentScore:F1}/10 ({ScoreLabel(recommendation.CurrentScore)})");
                });

                column.Item().Text(text =>
                {
                    text.Span("Por qué se recomienda: ").SemiBold().FontColor(Muted);
                    text.Span(recommendation.Reason).FontColor(Ink);
                });

                column.Item().Text(text =>
                {
                    text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Muted));
                    text.Span("Coste orientativo: ").SemiBold();
                    text.Span(recommendation.PriceRange);
                });

                if (!string.IsNullOrWhiteSpace(recommendation.CompatibilityNote))
                {
                    column.Item().Text(text =>
                    {
                        text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Muted));
                        text.Span("Antes de comprar: ").SemiBold();
                        text.Span(recommendation.CompatibilityNote);
                    });
                }

                if (recommendation.PurchaseLinks.Count > 0)
                {
                    column.Item().PaddingTop(2).Text(text =>
                    {
                        text.DefaultTextStyle(TextStyle.Default.FontSize(7).FontColor(Muted));
                        text.Span("Buscar: ").SemiBold();
                        for (int i = 0; i < recommendation.PurchaseLinks.Count; i++)
                        {
                            var link = recommendation.PurchaseLinks[i];
                            if (i > 0)
                            {
                                text.Span("   ");
                            }

                            text.Span($"{link.Store}: {link.Url}").FontColor(Primary);
                        }
                    });
                }
            });
        }

        private static void AddScoreCell(TableDescriptor table, double? score)
        {
            var value = score ?? 0;
            var color = ScoreColor(value);
            table.Cell().BorderBottom(1).BorderColor(Border).Padding(6)
                .Text(score.HasValue ? $"{value:F1}/10 · {ScoreLabel(value)}" : "N/A")
                .FontColor(color)
                .SemiBold();
        }

        private static void AddLabelCell(TableDescriptor table, string label)
        {
            table.Cell().BorderBottom(1).BorderColor(Border).Padding(6).Text(label).SemiBold().FontColor(Muted);
        }

        private static void AddValueCell(TableDescriptor table, string value)
        {
            table.Cell().BorderBottom(1).BorderColor(Border).Padding(6).Text(string.IsNullOrWhiteSpace(value) ? "N/A" : value).FontColor(Ink);
        }

        private static void AddHeaderCell(TableCellDescriptor table, string label)
        {
            table.Cell().Background(HeaderSurface).BorderBottom(1).BorderColor(Border).Padding(6)
                .Text(label.ToUpperInvariant()).FontSize(7).SemiBold().FontColor(Muted);
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

        private static void ComposeStatusBadge(IContainer container, string label, string color)
        {
            container.Background("#FFFFFF").Border(1).BorderColor(Border).PaddingHorizontal(6).PaddingVertical(2)
                .Text(label.ToUpperInvariant()).FontSize(6.5f).SemiBold().FontColor(color);
        }

        private static string PriorityColor(string priority)
            => priority.Contains("alta", StringComparison.OrdinalIgnoreCase)
                ? Critical
                : priority.Contains("media", StringComparison.OrdinalIgnoreCase)
                    ? Warning
                    : Primary;

        private static string ScoreColor(double score)
            => score < 4.5
                ? Critical
                : score < 6.5
                    ? Warning
                    : Primary;

        private static string ScoreLabel(double score)
            => score < 4.5
                ? "crítico"
                : score < 6.5
                    ? "mejorable"
                    : score < 8
                        ? "aceptable"
                        : "correcto";

        private static string ProfileLabel(UpgradeProfile profile)
            => profile switch
            {
                UpgradeProfile.General => "Uso ligero",
                UpgradeProfile.Gaming => "Uso medio",
                UpgradeProfile.HeavyWork => "Uso pesado",
                UpgradeProfile.VeryHeavyWork => "Uso muy pesado",
                _ => "Uso ligero"
            };

        private static string ProfileDescription(UpgradeProfile profile)
            => profile switch
            {
                UpgradeProfile.General => "Pensado para navegación, correo, ofimática, videollamadas, música, streaming y tareas diarias. Se prioriza fluidez, bajo coste y estabilidad, no rendimiento extremo.",
                UpgradeProfile.Gaming => "Pensado para multitarea frecuente, muchas pestañas, aplicaciones de estudio o trabajo, edición ligera y videojuegos casuales/eSports a 1080p. Se busca equilibrio entre CPU, RAM, GPU y almacenamiento.",
                UpgradeProfile.HeavyWork => "Pensado para videojuegos exigentes, edición de foto/vídeo, 3D, máquinas virtuales, compilación, análisis de datos y cargas sostenidas. Se valora margen de rendimiento y capacidad para trabajar durante más tiempo sin cuellos de botella.",
                UpgradeProfile.VeryHeavyWork => "Pensado para edición profesional avanzada, 3D complejo, IA local, datasets grandes, varias máquinas virtuales, renderizado, desarrollo pesado y multitarea extrema. Se prioriza mucho margen de CPU, RAM, VRAM y almacenamiento NVMe rápido.",
                _ => "Uso cotidiano del ordenador."
            };

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
