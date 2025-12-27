using Microsoft.AspNetCore.Mvc;
using SmartRestaurant.Application.Interfaces.Abstractions.AI;

namespace SmartRestaurant.API.Controllers;

[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbot;

    public ChatbotController(IChatbotService chatbot)
    {
        _chatbot = chatbot;
    }

    [HttpPost("ask")]
    public async Task<string> Ask([FromBody] string prompt, CancellationToken ct) 
        => await _chatbot.Ask(prompt, ct);
}