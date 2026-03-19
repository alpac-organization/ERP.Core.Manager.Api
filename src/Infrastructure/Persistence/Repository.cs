using System.Linq.Expressions;
using ERP.Core.Manager.Api.Domain.Commons.Interfaces;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context = context;
        private readonly DbSet<T> _dbSet = context.Set<T>();
        
        public IQueryable<T> Entities => _dbSet;

        public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => await _dbSet.FindAsync([id], ct);
        public async Task<List<T>> ToListAsync(IQueryable<T> query, CancellationToken ct) => await query.ToListAsync(ct);
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, ct);
        }
        public Task UpdateAsync(T entity)
        {
            // Usamos Task.CompletedTask porque marcar el estado es una operación síncrona,
            // pero exponemos la firma Task para que tu Handler use 'await'.
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            
            return Task.CompletedTask;
        }
    }
}   