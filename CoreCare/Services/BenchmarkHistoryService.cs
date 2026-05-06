using System;
using System.Collections.Generic;
using System.Linq;
using CoreCare.Data;
using CoreCare.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreCare.Services
{
    public sealed class BenchmarkHistoryService
    {
        private readonly CoreCareDbContext _db;

        public BenchmarkHistoryService(CoreCareDbContext db)
        {
            _db = db;
        }

        public List<RegistroBenchmark> GetHistory(int userId, DateTime? from, DateTime? to, int maxItems = 100)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "UserId must be a positive value.");
            }

            var query = _db.RegistrosBenchmark
                .AsNoTracking()
                .Where(record => record.UserId == userId)
                .OrderByDescending(record => record.Timestamp)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(record => record.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(record => record.Timestamp <= to.Value);
            }

            return query
                .Take(Math.Max(1, maxItems))
                .ToList();
        }

        public List<BenchmarkTrendPoint> BuildTrend(IReadOnlyCollection<RegistroBenchmark> history)
        {
            return history
                .OrderBy(record => record.Timestamp)
                .Select(record => new BenchmarkTrendPoint
                {
                    Timestamp = record.Timestamp,
                    Score = record.Score,
                    CpuTemp = record.CpuTemp,
                    GpuTemp = record.GpuTemp,
                    RamLoad = record.RamLoad,
                    DiskLoad = record.DiskLoad
                })
                .ToList();
        }

        public DegradationAnalysis AnalyzeDegradation(
            IReadOnlyCollection<RegistroBenchmark> history,
            int windowSize = 5,
            float scoreDropThresholdPercent = 10f)
        {
            if (history.Count < windowSize * 2)
            {
                return new DegradationAnalysis
                {
                    HasEnoughData = false,
                    IsDegraded = false,
                    Message = $"Datos insuficientes para analizar degradacion (minimo {windowSize * 2} ejecuciones)."
                };
            }

            var ordered = history
                .OrderBy(record => record.Timestamp)
                .ToList();

            var baselineWindow = ordered
                .Take(windowSize)
                .ToList();

            var recentWindow = ordered
                .TakeLast(windowSize)
                .ToList();

            float baselineScore = baselineWindow.Average(record => record.Score);
            float recentScore = recentWindow.Average(record => record.Score);

            if (baselineScore <= 0f)
            {
                return new DegradationAnalysis
                {
                    HasEnoughData = true,
                    IsDegraded = false,
                    BaselineAverageScore = baselineScore,
                    RecentAverageScore = recentScore,
                    ScoreDropPercent = 0f,
                    Message = "No es posible calcular degradacion porque el score base es cero."
                };
            }

            float scoreDropPercent = ((baselineScore - recentScore) / baselineScore) * 100f;
            bool degraded = scoreDropPercent >= scoreDropThresholdPercent;

            return new DegradationAnalysis
            {
                HasEnoughData = true,
                IsDegraded = degraded,
                BaselineAverageScore = baselineScore,
                RecentAverageScore = recentScore,
                ScoreDropPercent = scoreDropPercent,
                Message = degraded
                    ? $"Degradacion detectada: caida del score medio de {scoreDropPercent:F1}% (umbral {scoreDropThresholdPercent:F1}%)."
                    : $"Sin degradacion: variacion del score medio {scoreDropPercent:F1}% (umbral {scoreDropThresholdPercent:F1}%)."
            };
        }
    }

    public sealed class BenchmarkTrendPoint
    {
        public DateTime Timestamp { get; set; }
        public float Score { get; set; }
        public float CpuTemp { get; set; }
        public float GpuTemp { get; set; }
        public float RamLoad { get; set; }
        public float DiskLoad { get; set; }
    }

    public sealed class DegradationAnalysis
    {
        public bool HasEnoughData { get; set; }
        public bool IsDegraded { get; set; }
        public float BaselineAverageScore { get; set; }
        public float RecentAverageScore { get; set; }
        public float ScoreDropPercent { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
