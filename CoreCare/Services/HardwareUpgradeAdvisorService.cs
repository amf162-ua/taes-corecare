using System;
using System.Collections.Generic;
using System.Linq;
using CoreCare.Models;

namespace CoreCare.Services
{
    public sealed class HardwareUpgradeAdvisorService
    {
        public (List<ComponentScore> Scores, List<UpgradeRecommendation> Recommendations) Analyze(SystemSpecs specs, SystemTelemetryMock telemetry)
        {
            var scores = new List<ComponentScore>();

            foreach (UpgradeProfile profile in Enum.GetValues(typeof(UpgradeProfile)))
            {
                scores.Add(ScoreCpu(specs, profile));
                scores.Add(ScoreRam(specs, telemetry, profile));
                scores.Add(ScoreGpu(specs, profile));
                scores.Add(ScoreDisk(specs, telemetry, profile));
            }

            var recommendations = BuildRecommendations(specs, telemetry, scores);
            return (scores, recommendations);
        }

        private static ComponentScore ScoreCpu(SystemSpecs specs, UpgradeProfile profile)
        {
            var requiredCores = profile switch
            {
                UpgradeProfile.General => 2,
                UpgradeProfile.Gaming => 4,
                UpgradeProfile.HeavyWork => 8,
                UpgradeProfile.VeryHeavyWork => 12,
                _ => 2
            };
            var requiredThreads = profile switch
            {
                UpgradeProfile.General => 4,
                UpgradeProfile.Gaming => 8,
                UpgradeProfile.HeavyWork => 16,
                UpgradeProfile.VeryHeavyWork => 24,
                _ => 4
            };

            var coreScore = RatioScore(specs.CpuCores, requiredCores);
            var threadScore = RatioScore(specs.CpuThreads, requiredThreads);
            var clockScore = specs.CpuMaxClockMhz <= 0 ? 6 : RatioScore(specs.CpuMaxClockMhz, 4200);
            var score = Math.Round((coreScore * 0.45) + (threadScore * 0.4) + (clockScore * 0.15), 1);

            return new ComponentScore
            {
                Component = "CPU",
                Profile = profile,
                Score = score,
                Reason = $"Procesador detectado con {specs.CpuCores} núcleos y {specs.CpuThreads} hilos. Para {ProfileLabel(profile)} se toma como referencia {requiredCores} núcleos y {requiredThreads} hilos: por debajo de ese nivel pueden aparecer esperas al abrir varias aplicaciones, compilar, jugar o trabajar con tareas simultáneas."
            };
        }

        private static ComponentScore ScoreRam(SystemSpecs specs, SystemTelemetryMock telemetry, UpgradeProfile profile)
        {
            var totalGb = specs.RamTotalGb > 0 ? specs.RamTotalGb : telemetry.RamTotalGb;
            var requiredGb = profile switch
            {
                UpgradeProfile.General => 8,
                UpgradeProfile.Gaming => 16,
                UpgradeProfile.HeavyWork => 32,
                UpgradeProfile.VeryHeavyWork => 64,
                _ => 8
            };

            var capacityScore = RatioScore(totalGb, requiredGb);
            var usagePressure = telemetry.RamTotalGb > 0 ? telemetry.RamUsedGb / telemetry.RamTotalGb : 0;
            var pressurePenalty = usagePressure > 0.8 ? 1.5 : usagePressure > 0.65 ? 0.7 : 0;

            return new ComponentScore
            {
                Component = "RAM",
                Profile = profile,
                Score = Math.Round(Math.Max(0, capacityScore - pressurePenalty), 1),
                Reason = $"Memoria detectada: {totalGb:F1} GB. Para {ProfileLabel(profile)} se recomienda llegar al menos a {requiredGb} GB; si la RAM se queda corta, Windows empieza a usar el disco como memoria temporal y el equipo se nota más lento."
            };
        }

        private static ComponentScore ScoreGpu(SystemSpecs specs, UpgradeProfile profile)
        {
            var isIntegrated = IsIntegratedGpu(specs.GpuName);
            var requiredVram = profile switch
            {
                UpgradeProfile.General => 1,
                UpgradeProfile.Gaming => 4,
                UpgradeProfile.HeavyWork => 12,
                UpgradeProfile.VeryHeavyWork => 16,
                _ => 1
            };

            var vramScore = specs.GpuVramGb <= 0 ? (isIntegrated ? 3 : 5) : RatioScore(specs.GpuVramGb, requiredVram);
            var integratedPenalty = isIntegrated && profile != UpgradeProfile.General ? 2.5 : 0;

            return new ComponentScore
            {
                Component = "GPU",
                Profile = profile,
                Score = Math.Round(Math.Max(0, vramScore - integratedPenalty), 1),
                Reason = $"Gráfica detectada: {specs.GpuName}. VRAM detectada: {specs.GpuVram}. Para {ProfileLabel(profile)} se usa como referencia {requiredVram} GB de VRAM; una GPU integrada o con poca memoria limita juegos, edición, aceleración gráfica y cargas profesionales."
            };
        }

        private static ComponentScore ScoreDisk(SystemSpecs specs, SystemTelemetryMock telemetry, UpgradeProfile profile)
        {
            var isSsd = IsSsdLike(specs);
            var isNvme = IsNvmeLike(specs);
            var requiredSize = profile switch
            {
                UpgradeProfile.General => 250,
                UpgradeProfile.Gaming => 500,
                UpgradeProfile.HeavyWork => 1000,
                UpgradeProfile.VeryHeavyWork => 2000,
                _ => 250
            };
            var typeScore = isNvme ? 10 : isSsd ? 8 : 4;
            var sizeScore = specs.DiskSizeGb <= 0 ? 6 : RatioScore(specs.DiskSizeGb, requiredSize);
            var loadPenalty = telemetry.DiskUsagePercent > 85 ? 1 : 0;

            return new ComponentScore
            {
                Component = "Disco",
                Profile = profile,
                Score = Math.Round(Math.Max(0, (typeScore * 0.65) + (sizeScore * 0.35) - loadPenalty), 1),
                Reason = $"Unidad detectada: {specs.DiskModel}. Tipo/interfaz: {specs.DiskMediaType}/{specs.DiskInterfaceType}. Capacidad: {specs.DiskSize}. Un SSD, especialmente NVMe, reduce tiempos de arranque, carga de programas, actualizaciones y respuesta general frente a discos mecánicos o unidades pequeñas."
            };
        }

        private static List<UpgradeRecommendation> BuildRecommendations(SystemSpecs specs, SystemTelemetryMock telemetry, IReadOnlyCollection<ComponentScore> scores)
        {
            var result = new List<UpgradeRecommendation>();

            foreach (var score in scores.Where(score => score.Score < 6.5))
            {
                var current = score.Component switch
                {
                    "CPU" => specs.CpuName,
                    "RAM" => $"{Math.Max(specs.RamTotalGb, telemetry.RamTotalGb):F1} GB",
                    "GPU" => specs.GpuName,
                    "Disco" => $"{specs.DiskModel} ({specs.DiskSize})",
                    _ => string.Empty
                };

                var template = score.Component switch
                {
                    "CPU" => BuildCpuUpgrade(specs, score.Profile),
                    "RAM" => BuildRamUpgrade(specs, telemetry, score.Profile),
                    "GPU" => BuildGpuUpgrade(specs, score.Profile),
                    "Disco" => BuildDiskUpgrade(specs, score.Profile),
                    _ => null
                };

                if (template is null)
                {
                    continue;
                }

                result.Add(new UpgradeRecommendation
                {
                    Component = score.Component,
                    CurrentComponent = current,
                    Profile = score.Profile,
                    CurrentScore = score.Score,
                    Priority = score.Score < 4.5 ? "Alta" : "Media",
                    SuggestedUpgrade = template.Name,
                    Reason = $"{score.Reason} {template.Reason}",
                    PriceRange = template.PriceRange,
                    CompatibilityNote = template.CompatibilityNote,
                    PurchaseLinks = BuildPurchaseLinks(template.SearchQuery)
                });
            }

            return result
                .OrderBy(r => ProfileOrder(r.Profile))
                .ThenBy(r => r.CurrentScore)
                .ThenBy(r => r.Component)
                .ToList();
        }

        private static UpgradeTemplate BuildCpuUpgrade(SystemSpecs specs, UpgradeProfile profile)
            => profile switch
            {
                UpgradeProfile.General => new UpgradeTemplate(
                    "CPU moderna de bajo consumo con 4 núcleos / 8 hilos",
                    "Se recomienda porque este perfil no necesita una CPU de gama alta, pero sí una base moderna para navegar con varias pestañas, usar videollamadas y mantener el sistema fluido sin bloqueos.",
                    "100-220 EUR",
                    "Verificar socket, chipset, BIOS y refrigeración antes de comprar.",
                    "procesador 4 nucleos 8 hilos"),
                UpgradeProfile.Gaming => new UpgradeTemplate(
                    "CPU 6 núcleos / 12 hilos",
                    "Se recomienda porque ofrece margen para multitarea, aplicaciones pesadas moderadas y juegos actuales sin depender solo de pocos núcleos. Ayuda a mantener FPS más estables y menos tirones.",
                    "150-300 EUR",
                    "Verificar socket, chipset, BIOS y refrigeración antes de comprar.",
                    "procesador 6 nucleos 12 hilos gaming"),
                UpgradeProfile.HeavyWork => new UpgradeTemplate(
                    "CPU 8 núcleos / 16 hilos o superior",
                    "Se recomienda para cargas sostenidas y paralelas: edición, compilación, renderizado, virtualización y trabajo profesional. Más núcleos e hilos reducen esperas cuando varias tareas compiten por CPU.",
                    "250-550 EUR",
                    "Verificar socket, chipset, BIOS, fuente y refrigeración antes de comprar.",
                    "procesador 8 nucleos 16 hilos"),
                _ => new UpgradeTemplate(
                    "CPU 12 núcleos / 24 hilos o superior",
                    "Se recomienda para cargas muy pesadas y simultáneas: renderizado prolongado, varias máquinas virtuales, compilaciones grandes, análisis de datos e IA local. Este perfil necesita mucho margen para evitar bloqueos cuando varias tareas intensivas coinciden.",
                    "400-900 EUR",
                    "Verificar socket, chipset, BIOS, VRM de placa base, fuente y refrigeración de alto rendimiento antes de comprar.",
                    "procesador 12 nucleos 24 hilos")
            };

        private static UpgradeTemplate BuildRamUpgrade(SystemSpecs specs, SystemTelemetryMock telemetry, UpgradeProfile profile)
        {
            var total = Math.Max(specs.RamTotalGb, telemetry.RamTotalGb);
            var target = profile == UpgradeProfile.VeryHeavyWork ? "64GB RAM DDR4 DDR5" : profile == UpgradeProfile.HeavyWork ? "32GB RAM DDR4 DDR5" : total < 16 ? "16GB RAM DDR4 DDR5" : "32GB RAM DDR4 DDR5";
            var targetGb = profile == UpgradeProfile.VeryHeavyWork ? 64 : profile == UpgradeProfile.HeavyWork || total >= 16 ? 32 : 16;
            return new UpgradeTemplate(
                $"Ampliar a {targetGb} GB de RAM",
                "Se recomienda porque la RAM insuficiente provoca paginación: el sistema usa el disco como apoyo y todo responde peor. La ampliación aporta margen para pestañas, aplicaciones abiertas, juegos, edición y procesos en segundo plano.",
                targetGb >= 64 ? "140-300 EUR" : targetGb >= 32 ? "70-150 EUR" : "35-80 EUR",
                "Comprobar DDR4/DDR5, velocidad soportada y ranuras libres.",
                target);
        }

        private static UpgradeTemplate BuildGpuUpgrade(SystemSpecs specs, UpgradeProfile profile)
            => profile switch
            {
                UpgradeProfile.General => new UpgradeTemplate(
                    "GPU integrada moderna o dedicada básica",
                    "Se recomienda solo si la GPU actual es muy limitada. Para uso ligero basta con una gráfica capaz de mover vídeo, monitores externos y aceleración básica sin consumir muchos recursos.",
                    "100-220 EUR",
                    "En portátiles normalmente no es reemplazable; verificar formato y fuente.",
                    "tarjeta grafica bajo consumo multimedia"),
                UpgradeProfile.Gaming => new UpgradeTemplate(
                    "GPU dedicada con 8 GB de VRAM",
                    "Se recomienda porque la GPU suele ser el principal límite en videojuegos y aplicaciones aceleradas. 8 GB de VRAM dan margen razonable para 1080p, texturas actuales y edición ligera.",
                    "250-450 EUR",
                    "Verificar espacio en caja, fuente de alimentación y conectores PCIe.",
                    "tarjeta grafica 8GB VRAM gaming"),
                UpgradeProfile.HeavyWork => new UpgradeTemplate(
                    "GPU moderna con 12 GB de VRAM o más",
                    "Se recomienda para edición avanzada, renderizado, IA local y proyectos con escenas o archivos grandes. Más VRAM evita cuellos de botella cuando los datos no caben en memoria gráfica.",
                    "450-900 EUR",
                    "Verificar fuente, tamaño, refrigeración y compatibilidad de software.",
                    "tarjeta grafica 12GB VRAM"),
                _ => new UpgradeTemplate(
                    "GPU profesional o gama alta con 16 GB de VRAM o más",
                    "Se recomienda para IA local, renderizado complejo, edición 4K/8K, simulación, escenas 3D grandes y trabajo profesional con datasets pesados. En este perfil la VRAM evita que los proyectos se queden sin memoria gráfica.",
                    "700-1600 EUR",
                    "Verificar fuente, tamaño, refrigeración, conectores PCIe, compatibilidad CUDA/OpenCL y si el equipo permite realmente cambiar la GPU.",
                    "tarjeta grafica 16GB VRAM profesional")
            };

        private static UpgradeTemplate BuildDiskUpgrade(SystemSpecs specs, UpgradeProfile profile)
            => new(
                profile == UpgradeProfile.VeryHeavyWork ? "SSD NVMe 4 TB de alto rendimiento" : profile == UpgradeProfile.HeavyWork ? "SSD NVMe 2 TB" : IsSsdLike(specs) ? "SSD NVMe 1 TB de mayor rendimiento" : "SSD 1 TB para sistema y aplicaciones",
                "Se recomienda porque el almacenamiento afecta a arranque, apertura de programas, cargas de juegos, actualizaciones y trabajo con archivos grandes. Pasar a SSD/NVMe suele ser una de las mejoras más visibles en equipos lentos.",
                profile == UpgradeProfile.VeryHeavyWork ? "220-500 EUR" : profile == UpgradeProfile.HeavyWork ? "110-250 EUR" : "55-130 EUR",
                "Si el equipo no admite NVMe, elegir SSD SATA 2.5 pulgadas.",
                profile == UpgradeProfile.VeryHeavyWork ? "SSD NVMe 4TB alto rendimiento" : profile == UpgradeProfile.HeavyWork ? "SSD NVMe 2TB" : IsSsdLike(specs) ? "SSD NVMe 1TB" : "SSD SATA 1TB");

        private static List<PurchaseLink> BuildPurchaseLinks(string query)
        {
            var encoded = Uri.EscapeDataString(query);
            return new List<PurchaseLink>
            {
                new() { Store = "Amazon.es", Url = $"https://www.amazon.es/s?k={encoded}" },
                new() { Store = "PcComponentes", Url = $"https://www.pccomponentes.com/search/?query={encoded}" }
            };
        }

        private static double RatioScore(double value, double required)
        {
            if (value <= 0 || required <= 0)
            {
                return 0;
            }

            return Math.Clamp((value / required) * 10, 0, 10);
        }

        private static bool IsIntegratedGpu(string name)
            => name.Contains("Intel", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Radeon Graphics", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("UHD", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Iris", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Vega", StringComparison.OrdinalIgnoreCase);

        private static bool IsSsdLike(SystemSpecs specs)
            => specs.DiskMediaType.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
               specs.DiskModel.Contains("SSD", StringComparison.OrdinalIgnoreCase) ||
               IsNvmeLike(specs);

        private static bool IsNvmeLike(SystemSpecs specs)
            => specs.DiskInterfaceType.Contains("NVMe", StringComparison.OrdinalIgnoreCase) ||
               specs.DiskModel.Contains("NVMe", StringComparison.OrdinalIgnoreCase);

        private static string ProfileLabel(UpgradeProfile profile)
            => profile switch
            {
                UpgradeProfile.General => "uso ligero",
                UpgradeProfile.Gaming => "uso medio",
                UpgradeProfile.HeavyWork => "uso pesado",
                UpgradeProfile.VeryHeavyWork => "uso muy pesado",
                _ => "uso ligero"
            };

        private static int ProfileOrder(UpgradeProfile profile)
            => profile switch
            {
                UpgradeProfile.General => 0,
                UpgradeProfile.Gaming => 1,
                UpgradeProfile.HeavyWork => 2,
                UpgradeProfile.VeryHeavyWork => 3,
                _ => 0
            };

        private sealed record UpgradeTemplate(string Name, string Reason, string PriceRange, string CompatibilityNote, string SearchQuery);
    }
}
