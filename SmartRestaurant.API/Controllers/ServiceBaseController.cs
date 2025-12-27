using Microsoft.AspNetCore.Mvc;
using SmartRestaurant.Application.Interfaces.Common;

namespace SmartRestaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ServiceBaseController<T> : ControllerBase where T : class
{
    protected readonly IServiceBase<T> _service;

    protected ServiceBaseController(IServiceBase<T> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<List<T>> GetAll() => await _service.GetAllAsync();

    [HttpGet("{id:guid}")]
    public async Task<T> GetById(Guid id) => await _service.GetByIdAsync(id);

    [HttpPost]
    public async Task Create([FromBody] T entity, CancellationToken ct)  => await _service.AddAsync(entity, ct);

    [HttpPut("{id:guid}")]
    public async Task Update([FromBody] T entity, CancellationToken ct) => await _service.UpdateAsync(entity, ct);

    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken ct) => await _service.DeleteAsync(id, ct);
}