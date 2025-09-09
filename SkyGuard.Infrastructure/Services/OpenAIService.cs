using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;

namespace SkyGuard.Infrastructure.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly double _temperature;
        private readonly int _cacheDurationMinutes;

        public OpenAIService(IConfiguration configuration, IMemoryCache cache, ILogger<OpenAIService> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;

            // Configure TLS settings globally
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;

            // Create HttpClient with custom handler
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };


            _httpClient = new HttpClient(handler);

            var apiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("OpenAI API key is not configured");
            }

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DroneSurveillanceApp/1.0");

            // Set timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Parse configuration
            _model = configuration["OpenAI:Model"] ?? "gpt-4-turbo-preview";
            _maxTokens = int.Parse(configuration["OpenAI:MaxTokens"] ?? "2000");
            _temperature = double.Parse(configuration["OpenAI:Temperature"] ?? "0.3");
            _cacheDurationMinutes = int.Parse(configuration["OpenAI:CacheDurationMinutes"] ?? "30");
        }

        public async Task<string> AnalyzeIncidentsAsync(List<Incident> incidents, List<SecurityResponse> responses, string analysisType)
        {
            var cacheKey = $"analysis_{analysisType}_{GetDataHash(incidents, responses)}";

            if (_cache.TryGetValue(cacheKey, out string cachedResult))
            {
                _logger.LogInformation("Returning cached analysis result");
                return cachedResult;
            }

            try
            {
                var systemPrompt = GetSystemPrompt(analysisType);
                var userPrompt = BuildAnalysisPrompt(incidents, responses, analysisType);

                var requestData = new
                {
                    model = _model,
                    messages = new[]
                    {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                    max_tokens = _maxTokens,
                    temperature = _temperature,
                    response_format = new { type = "text" }
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestData);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (openAIResponse?.choices == null || openAIResponse.choices.Length == 0)
                {
                    throw new ApplicationException("Invalid response from OpenAI API");
                }

                var result = openAIResponse.choices[0].message.content;

                // Cache the result
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheDurationMinutes));

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OpenAI API request failed");
                throw new ApplicationException("AI analysis service unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI analysis");
                throw;
            }
        }

        public async Task<T> AnalyzeWithStructuredResponseAsync<T>(List<Incident> incidents, List<SecurityResponse> responses, string systemPrompt) where T : class
        {
            var cacheKey = $"structured_{typeof(T).Name}_{GetDataHash(incidents, responses)}";

            if (_cache.TryGetValue(cacheKey, out T cachedResult))
            {
                return cachedResult;
            }

            try
            {
                var userPrompt = BuildAnalysisPrompt(incidents, responses, "structured");

                var requestData = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    max_tokens = _maxTokens,
                    temperature = _temperature,
                    response_format = new { type = "json_object" }
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestData);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (openAIResponse?.choices == null || openAIResponse.choices.Length == 0)
                {
                    throw new ApplicationException("Invalid response from OpenAI API");
                }

                var resultJson = openAIResponse.choices[0].message.content;

                // Handle cases where the response might be wrapped in code blocks
                resultJson = CleanJsonResponse(resultJson);

                // --- FIX: If T is a List type, deserialize as a JSON array ---
                object? result;
                var targetType = typeof(T);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // If the response is an object with a property containing the array, extract it
                    using var doc = JsonDocument.Parse(resultJson);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        result = JsonSerializer.Deserialize(resultJson, targetType, options);
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        // Try to find the first array property
                        var arrayProp = doc.RootElement.EnumerateObject()
                            .FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);
                        if (arrayProp.Value.ValueKind == JsonValueKind.Array)
                        {
                            result = JsonSerializer.Deserialize(arrayProp.Value.GetRawText(), targetType, options);
                        }
                        else
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                else
                {
                    result = JsonSerializer.Deserialize<T>(resultJson, options);
                }

                if (result == null)
                {
                    throw new ApplicationException("Failed to deserialize AI response");
                }

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheDurationMinutes));

                return (T)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during structured AI analysis");
                throw;
            }
        }
        public async Task<bool> ValidateConfiguration()
        {         

            try
            {
  
                // Test the configuration by making a simple models request
                var response = await _httpClient.GetAsync("models");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI API request failed. Inner: {Inner}", ex.InnerException?.Message);
                return false;
            }
        }

        private string GetSystemPrompt(string analysisType)
        {
            return analysisType.ToLower() switch
            {
                "dashboard" => @"You are a security analysis expert for pipeline surveillance. 
                Analyze incident data and provide comprehensive insights including:
                - Hotspot identification with areas of highest incident concentration
                - Risk prediction with confidence levels
                - Actionable recommendations for resource allocation
                - Pattern analysis including peak hours and trends
                Return your analysis in structured JSON format.",

                "recommendations" => @"You are a security operations advisor. Provide specific, 
                actionable recommendations based on incident patterns. Include priority levels, 
                categories, and expected impact. Return JSON array of recommendations.",

                "risk" => @"You are a risk assessment specialist. Predict future security risks 
                based on historical incident data and current trends. Return JSON object with risk prediction.",

                "hotspots" => @"You are a geospatial analyst. Identify security hotspots based on 
                incident locations and patterns. Return JSON object with hotspot analysis.",

                "patterns" => @"You are a data pattern analyst. Identify temporal and spatial patterns 
                in security incidents. Return JSON object with pattern analysis.",

                _ => @"You are an AI assistant analyzing drone surveillance data. Provide 
                insightful analysis based on the provided incident and response data."
            };
        }

        private string  BuildAnalysisPrompt(List<Incident> incidents, List<SecurityResponse> responses, string analysisType)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine($"Analysis Type: {analysisType}");
            prompt.AppendLine($"Total Incidents: {incidents.Count}");
            prompt.AppendLine($"Total Responses: {responses.Count}");
            prompt.AppendLine();

            prompt.AppendLine("Recent Incidents Summary:");
            foreach (var incident in incidents.Take(10))
            {
                prompt.AppendLine($"- {incident.Title} ({incident.Area}): {incident.Description}");
                prompt.AppendLine($"  Location: {incident.Latitude}, {incident.Longitude}");
                prompt.AppendLine($"  Priority: {incident.Priority}, Status: {incident.Status}");
                prompt.AppendLine();
            }

            if (responses.Any())
            {
                prompt.AppendLine("Response Summary:");
                foreach (var response in responses.Take(10))
                {
                    prompt.AppendLine($"- Confirmation: {response.Classification}");
                    prompt.AppendLine($"  Notes: {response.AdditionalComments}");
                    prompt.AppendLine($"  Responded By: {response.RespondedBy}");
                    prompt.AppendLine();
                }
            }

            prompt.AppendLine("Please provide comprehensive analysis with specific insights and recommendations.");

            return prompt.ToString();
        }

        private string CleanJsonResponse(string jsonResponse)
        {
            // Remove markdown code blocks if present
            if (jsonResponse.Contains("```json"))
            {
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();
            }
            else if (jsonResponse.Contains("```"))
            {
                jsonResponse = jsonResponse.Replace("```", "").Trim();
            }

            return jsonResponse;
        }

        private string GetDataHash(List<Incident> incidents, List<SecurityResponse> responses)
        {
            // Create a simple hash for caching based on data content
            var incidentIds = string.Join(",", incidents.Select(i => i.Id));
            var responseIds = string.Join(",", responses.Select(r => r.Id));
            return $"{incidentIds.GetHashCode()}_{responseIds.GetHashCode()}";
        }
    }

    // Response classes for OpenAI API
    public class OpenAIResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
