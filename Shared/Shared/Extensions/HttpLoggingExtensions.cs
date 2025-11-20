using Microsoft.AspNetCore.Http;
using Shared.Model;
using System.Text.Json;
namespace Shared.Extensions
{
    public static class HttpLoggingExtensions
    {
        public static async Task<string> ConvertRequestToJsonAsync(this HttpRequest request)
        {
            request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            request.Body.Position = 0;

            var requestLog = new RequestLog
            {
                Method = request.Method,
                Path = request.Path,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = body
            };

            return JsonSerializer.Serialize(requestLog, new JsonSerializerOptions { WriteIndented = true });
        }

        public static async Task<string> ConvertResponseToJsonAsync(this HttpResponse response)
        {
            // Leer el cuerpo de la respuesta
            response.Body.Seek(0, SeekOrigin.Begin); // Reiniciar el stream para leerlo
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin); // Reiniciar el stream para enviarlo al cliente

            // Crear el objeto ResponseLog
            var responseLog = new ResponseLog
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = body
            };

            // Serializar a JSON
            return JsonSerializer.Serialize(responseLog, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}