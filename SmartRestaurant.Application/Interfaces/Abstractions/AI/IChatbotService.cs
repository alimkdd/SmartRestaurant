namespace SmartRestaurant.Application.Interfaces.Abstractions.AI;

public interface IChatbotService
{
    Task<string> Ask(string prompt, CancellationToken ct = default);
}