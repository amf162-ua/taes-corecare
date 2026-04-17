using System;
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

        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiAIService()
        {
            _httpClient = new HttpClient();
            ApiKey = LoadApiKey();
        }

        private string LoadApiKey()
        {
            string? key = null;

            string envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env");
            if (File.Exists(envFilePath))
            {
                foreach (var line in File.ReadAllLines(envFilePath))
                {
                    if (line.StartsWith("GEMINI_API_KEY="))
                    {
                        key = line.Substring("GEMINI_API_KEY=".Length).Trim();
                        break;
                    }
                }
            }

            key ??= Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
            key ??= Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.Process);

            return key ?? "CLAVE_NO_CONFIGURADA";
        }

        public async Task<string> GetRecommendationsAsync(SystemTelemetryMock data)
        {
            // Serialize telemetry data to string
            string jsonData = JsonSerializer.Serialize(data);

            // Build the prompt for Gemini
            string promptMessage = $"Eres un experto informático. Analiza los siguientes datos de telemetría de un PC y devuelve un array JSON con recomendaciones de mejora, teniendo en cuenta coste e impacto. Datos: {jsonData}";

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

            string jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Make the request
            string requestUrl = $"{ApiUrl}?key={ApiKey}";
            HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, content);

            // Read and return the response
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }
        }
    }
}
