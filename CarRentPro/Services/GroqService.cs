using System.Text;
using System.Text.Json;
using CarRentPro.Models; 
using Microsoft.Extensions.Configuration;

namespace CarRentPro.Services
{
    public class GroqService : IGroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["GroqApiKey"];
        }

        public async Task<string> GetRecommendationAsync(string userQuery, string vehicleContext)
        {
            var requestUrl = "https://api.groq.com/openai/v1/chat/completions";

            var systemPrompt =
                "You are a helpful and professional car rental assistant for 'CarRent Pro'. " +
                "Here is the list of available vehicles (Format: ID | Name | Details): " +
                $"{vehicleContext}. " +
                "INSTRUCTIONS:" +
                "1. LANGUAGE DETECTION: If the user writes in English, reply in English. If the user writes in Romanian, reply in Romanian." +
                "2. LINK GENERATION: When you recommend a specific car, you MUST format its name as a clickable HTML link using this format: " +
                "<a href='/Vehicle/Details/{ID}' target='_blank' style='color: #0d6efd; font-weight: bold; text-decoration: underline;'>{Brand} {Model}</a> " +
                "(Replace {ID} with the ID from the list provided)." +
                "3. STYLE: Be persuasive but concise. Do not invent cars not on the list.";


            var requestBody = new GroqRequest
            {
                Model = "llama-3.3-70b-versatile", 
                Messages = new List<Message>
                {
                    new Message { Role = "system", Content = systemPrompt },
                    new Message { Role = "user", Content = userQuery }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var groqResponse = JsonSerializer.Deserialize<GroqResponse>(responseString, options);
                    return groqResponse?.Choices?[0]?.Message?.Content ?? "I cannot provide a recommendation right now.";
                }
                else
                {
                    return $"API Error ({response.StatusCode}): {responseString}";
                }
            }
            catch (Exception ex)
            {
                return $"Internal Server Error: {ex.Message}";
            }
        }
    }
}