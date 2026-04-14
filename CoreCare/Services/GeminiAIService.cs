using System;
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

        // Usar una variable de entorno para mantener segura la API de Gemini
        private readonly string ApiKey; 

        // URL del modelo Gemini 1.5 Flash
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiAIService()
        {
            _httpClient = new HttpClient();
            // Intenta obtener la clave de las variables de entorno del usuario o del sistema
            ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User) 
                     ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.Process) 
                     ?? "CLAVE_NO_CONFIGURADA";
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
