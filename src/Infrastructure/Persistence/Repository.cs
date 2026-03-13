using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context = context;
        private readonly DbSet<T> _dbSet = context.Set<T>();
        
        public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => await _dbSet.FindAsync([id], ct);
    }
}