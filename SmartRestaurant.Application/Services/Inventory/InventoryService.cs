using Microsoft.EntityFrameworkCore;
using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Application.Interfaces.Abstractions.AI;
using SmartRestaurant.Application.Services.Common;
using SmartRestaurant.Domain.Models;
using SmartRestaurant.Infrastructure.Context;

namespace SmartRestaurant.Application.Services.Inventory;

public class InventoryService : ServiceBase<InventoryItem>, IInventoryService
{
    private readonly IChatbotService _chatbot;

    public InventoryService(AppDbContext context, IChatbotService chatbot) : base(context)
    {
        _chatbot = chatbot;
    }

    public async Task DeductIngredients(List<OrderItem> orderItems, CancellationToken ct = default)
    {
        foreach (var orderItem in orderItems)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Ingredients)
                .FirstOrDefaultAsync(m => m.Id == orderItem.MenuItemId, ct);

            if (menuItem == null) continue;

            foreach (var ingredient in menuItem.Ingredients)
            {
                var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Name == ingredient.Name, ct);
                if (inventoryItem == null) continue;

                inventoryItem.Quantity -= ingredient.Quantity * orderItem.Quantity;
                inventoryItem.LastUpdated = DateTime.UtcNow;

                if (inventoryItem.Quantity < inventoryItem.MinimumThreshold)
                {
                    var prompt = $"Inventory item '{inventoryItem.Name}' is below minimum threshold ({inventoryItem.Quantity}). Suggest reorder quantity.";
                    var suggestion = await _chatbot.Ask(prompt, ct);
                    Console.WriteLine($"AI Suggestion for {inventoryItem.Name}: {suggestion}");
                }
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<InventoryItem>> GetLowStockItems()
    {
        return await _context.InventoryItems
            .Where(i => i.Quantity < i.MinimumThreshold)
            .ToListAsync();
    }
}