using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Aura.Gemini;

namespace Aura
{
    public class GeminiApiService
    {
        private const string ApiKeyHeader = "x-goog-api-key";
        private const string TextApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
        private const string VisionApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-vision:generateContent";

        private readonly HttpClient _httpClient;
        private readonly Settings _settings;

        public GeminiApiService(Settings settings)
        {
            _settings = settings;

            var handler = new HttpClientHandler();
            if (_settings.ProxyEnabled && !string.IsNullOrWhiteSpace(_settings.ProxyAddress))
            {
                handler.Proxy = new WebProxy(_settings.ProxyAddress);
                handler.UseProxy = true;
            }

            _httpClient = new HttpClient(handler);
        }

        public async Task<string> GenerateContentAsync(object content)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                return "Error: Gemini API Key is not set in the settings.";
            }

            try
            {
                var request = BuildRequest(content);
                var requestUrl = content is string ? TextApiUrl : VisionApiUrl;

                var jsonRequest = JsonSerializer.Serialize(request);
                var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequestMessage.Headers.Add(ApiKeyHeader, _settings.ApiKey);
                httpRequestMessage.Content = httpContent;

                var response = await _httpClient.SendAsync(httpRequestMessage);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);

                return geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "No content generated.";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        private GeminiRequest BuildRequest(object content)
        {
            var parts = new List<Part>();
            string prompt = _settings.CustomPrompt;

            if (content is string textContent)
            {
                parts.Add(new Part { Text = prompt.Replace("{input}", textContent) });
            }
            else if (content is BitmapSource imageContent)
            {
                parts.Add(new Part { Text = prompt.Replace("{input}", "") }); // Prompt first
                parts.Add(new Part { InlineData = new InlineData { MimeType = "image/png", Data = ConvertImageToBase64(imageContent) } });
            }

            return new GeminiRequest { Contents = new List<Content> { new Content { Parts = parts } } };
        }

        private string ConvertImageToBase64(BitmapSource bitmapSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}
