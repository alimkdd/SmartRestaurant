using Microsoft.EntityFrameworkCore;
using SmartRestaurant.Application.Interfaces.Common;
using SmartRestaurant.Domain.Models.Common;
using SmartRestaurant.Infrastructure.Context;

namespace SmartRestaurant.Application.Services.Common
{
    public abstract class ServiceBase<T> : IServiceBase<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;

        protected ServiceBase(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public async Task<T> GetByIdAsync(Guid id)
            => await _context.Set<T>().FindAsync(new object[] { id });

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            var existing = await _context.Set<T>().FindAsync(new object[] { entity.Id }, ct);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync(ct);
            }
        }


        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}