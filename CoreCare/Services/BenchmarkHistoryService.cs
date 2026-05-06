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

        // Método para guardar resultados en la base de datos
        public void SaveBenchmark(RegistroBenchmark result)
        {
            try
            {
                _db.RegistrosBenchmark.Add(result);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar en la base de datos: " + ex.Message);
            }
        }

        // Recuperar el historial filtrado por usuario
        public List<RegistroBenchmark> GetHistory(int userId, DateTime? from, DateTime? to, int maxItems = 100)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var query = _db.RegistrosBenchmark
                .AsNoTracking()
                .Where(record => record.UserId == userId)
                .OrderByDescending(record => record.Timestamp)
                .AsQueryable();

            if (from.HasValue) query = query.Where(record => record.Timestamp >= from.Value);
            if (to.HasValue) query = query.Where(record => record.Timestamp <= to.Value);

            return query.Take(Math.Max(1, maxItems)).ToList();
        }

        // Analizar si el rendimiento del PC está bajando
        public DegradationAnalysis AnalyzeDegradation(IReadOnlyCollection<RegistroBenchmark> history, int windowSize = 5)
        {
            if (history.Count < windowSize * 2)
            {
                // SOLUCIÓN: Añadido HasEnoughData = false
                return new DegradationAnalysis { IsDegraded = false, Message = "Datos insuficientes.", HasEnoughData = false };
            }

            var ordered = history.OrderBy(r => r.Timestamp).ToList();
            var baseline = ordered.Take(windowSize).Average(r => r.Score);
            var recent = ordered.TakeLast(windowSize).Average(r => r.Score);

            float drop = ((baseline - recent) / baseline) * 100f;
            bool isDegraded = drop >= 10f;

            return new DegradationAnalysis
            {
                IsDegraded = isDegraded,
                ScoreDropPercent = drop,
                Message = isDegraded ? $"Rendimiento bajo un {drop:F1}%" : "Rendimiento estable",
                HasEnoughData = true // SOLUCIÓN: Añadido HasEnoughData = true
            };
        }

        // SOLUCIÓN: Añadido el método BuildTrend que pedía el MainViewModel
        public List<BenchmarkTrendPoint> BuildTrend(IReadOnlyCollection<RegistroBenchmark> history)
        {
            if (history == null || history.Count == 0)
                return new List<BenchmarkTrendPoint>();

            return history.Select(h => new BenchmarkTrendPoint
            {
                Timestamp = h.Timestamp,
                Score = h.Score
            })
            .OrderBy(x => x.Timestamp)
            .ToList();
        }
    }

    public sealed class DegradationAnalysis
    {
        public bool IsDegraded { get; set; }
        public float ScoreDropPercent { get; set; }
        public string Message { get; set; } = string.Empty;

        // SOLUCIÓN: Añadida la propiedad que faltaba
        public bool HasEnoughData { get; set; }
    }

    // SOLUCIÓN: Añadida la clase de puntos de tendencia para la gráfica
    public sealed class BenchmarkTrendPoint
    {
        public DateTime Timestamp { get; set; }
        public float Score { get; set; }
    }
}