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
        
        public IQueryable<T> Entities => _context.Set<T>();

        public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => await _dbSet.FindAsync([id], ct);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, ct);
        }
    }
}   