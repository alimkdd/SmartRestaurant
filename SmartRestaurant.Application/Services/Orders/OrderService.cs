using Microsoft.EntityFrameworkCore;
using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Application.Interfaces.Abstractions.AI;
using SmartRestaurant.Application.Services.Common;
using SmartRestaurant.Domain.Models;
using SmartRestaurant.Infrastructure.Context;

namespace SmartRestaurant.Application.Services.Orders;

public class OrderService : ServiceBase<Order>, IOrderService
{
    private readonly IChatbotService _chatbot;

    public OrderService(AppDbContext context, IChatbotService chatbot)
        : base(context)
    {
        _chatbot = chatbot;
    }

    public async Task<List<Order>> GetOrdersByCustomer(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<string> RecommendMenu(Guid customerId, CancellationToken ct = default)
    {
        // Get previous orders
        var previousOrders = await GetOrdersByCustomer(customerId);
        var orderedItems = previousOrders.SelectMany(o => o.Items)
            .Select(oi => _context.MenuItems.Find(oi.MenuItemId)?.Name)
            .Where(n => n != null)
            .ToList();

        var prompt = $"Customer previously ordered: {string.Join(", ", orderedItems)}.\n" +
                     "Recommend a menu item for this customer based on their preferences.";

        var recommendation = await _chatbot.Ask(prompt, ct);
        return recommendation;
    }

}