using Microsoft.Extensions.Configuration;
using SmartRestaurant.Application.Interfaces.Abstractions.AI;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartRestaurant.Application.Services.AI;

public sealed class ChatbotService : IChatbotService
{
    private readonly HttpClient _http;
    private readonly string _model;

    public ChatbotService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri(config["AI:BaseUrl"]!); // e.g., http://localhost:1234/v1/
        _model = config["AI:Model"]!;
    }

    public async Task<string> Ask(string prompt, CancellationToken ct = default)
    {
        try
        {
            // Build payload for local server
            var payload = new
            {
                model = _model,
                prompt = prompt,
                temperature = 0.7
            };

            // POST to /v1/completions
            var response = await _http.PostAsJsonAsync("completions", payload, ct);

            // Read raw JSON (for debugging)
            var jsonString = await response.Content.ReadAsStringAsync(ct);
            Console.WriteLine("Response JSON: " + jsonString);

            response.EnsureSuccessStatusCode();

            // Parse JSON response
            var json = JsonSerializer.Deserialize<JsonElement>(jsonString);

            if (json.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var text = choices[0].GetProperty("text").GetString();
                return text ?? "No content returned from model.";
            }

            return "No choices returned from model.";
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in ChatbotService.Ask: " + ex);
            throw;
        }
    }
}