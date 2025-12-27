using Microsoft.AspNetCore.Mvc;
using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Domain.Enums;
using SmartRestaurant.Domain.Models;

namespace SmartRestaurant.API.Controllers;

public class OrderController : ServiceBaseController<Order>
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService service) : base(service)
    {
        _orderService = service;
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
        => Ok(await _orderService.GetOrdersByCustomer(customerId));

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order is null) return NotFound();

        order.Status = status;
        await _orderService.UpdateAsync(order);

        return NoContent();
    }
}