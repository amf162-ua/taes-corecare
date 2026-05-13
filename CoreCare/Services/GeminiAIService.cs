using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CoreCare.Models;

namespace CoreCare.Services
{
    public class GeminiAIService
    {
        private readonly HttpClient _httpClient;

        private readonly string ApiKey;

        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1/models";
        private static readonly string[] ModelFallbackOrder =
        {
            "gemini-2.5-flash",
            "gemini-2.5-flash-lite",
            "gemini-3.1-flash-lite"
        };

        public GeminiAIService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            ApiKey = LoadApiKey();
        }

        private string LoadApiKey()
        {
            string? key = null;

            string[] possiblePaths = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".env"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
                Path.Combine(Directory.GetCurrentDirectory(), ".env")
            };

            foreach (var envFilePath in possiblePaths)
            {
                var fullPath = Path.GetFullPath(envFilePath);
                if (File.Exists(fullPath))
                {
                    foreach (var line in File.ReadAllLines(fullPath))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("GEMINI_API_KEY="))
                        {
                            key = trimmed.Substring("GEMINI_API_KEY=".Length).Trim();
                            if (!string.IsNullOrEmpty(key)) break;
                        }
                    }
                    if (key != null) break;
                }
            }

            key ??= Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
            key ??= Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.Process);

            return key ?? "CLAVE_NO_CONFIGURADA";
        }

        public async Task<string> GetRecommendationsAsync(SystemTelemetryMock data, SystemSpecs specs)
        {
            var combined = new
            {
                telemetry = data,
                specs = specs
            };
            string jsonData = JsonSerializer.Serialize(combined);

            string promptMessage = $@"Eres un experto tecnológico. Analiza los datos de telemetría y las especificaciones del sistema proporcionadas y da recomendaciones de mejora para el cliente.
Formato SIN asteriscos, SIN Markdown, texto plano limpio:

=== PRIORIDAD ALTA ===
[Componente que necesita atención]
- Problema: [qué está mal o puede mejorar]
- Solución: [qué hacer]
- Coste: Bajo / Medio / Alto
- Impacto: Bajo / Medio / Alto

=== PRIORIDAD MEDIA ===
[Otro componente]
- Problema: [...]
- Solución: [...]
- Coste: ...
- Impacto: ...

=== NOTAS ADICIONALES ===
[Consejos extra si aplica]

Datos de sistema: {jsonData}";
            promptMessage += "\nNota: los valores numéricos -1 indican que el sensor no está disponible y no deben interpretarse como carga o temperatura real.";

            // Payload for Gemini API 
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptMessage }
                        }
                    }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", ApiKey);

            var errors = new List<string>();

            foreach (var model in ModelFallbackOrder)
            {
                try
                {
                    string requestUrl = $"{BaseUrl}/{model}:generateContent";
                    string jsonPayload = JsonSerializer.Serialize(payload);
                    using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    using HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content);
                    string responseJson = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var parsed = JsonSerializer.Deserialize<GeminiResponse>(responseJson);
                            return parsed?.candidates?[0]?.content?.parts?[0]?.text ?? responseJson;
                        }
                        catch
                        {
                            return responseJson;
                        }
                    }

                    errors.Add($"{model}: {response.StatusCode} - {responseJson}");
                }
                catch (TaskCanceledException ex)
                {
                    errors.Add($"{model}: Timeout - {ex.Message}");
                }
                catch (HttpRequestException ex)
                {
                    errors.Add($"{model}: Fallo de red - {ex.Message}");
                }
            }

            return errors.Count == 0
                ? "Error: No se pudo obtener respuesta de Gemini."
                : "Error: No se pudo obtener respuesta de Gemini. Intentos: " + string.Join(" | ", errors);
        }
    }

    internal class GeminiResponse
    {
        public List<Candidate>? candidates { get; set; }
    }

    internal class Candidate
    {
        public Content? content { get; set; }
    }

    internal class Content
    {
        public List<Part>? parts { get; set; }
    }

    internal class Part
    {
        public string? text { get; set; }
    }
}
