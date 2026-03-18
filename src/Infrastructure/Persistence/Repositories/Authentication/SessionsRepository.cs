using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication
{
    public class SessionsRepository(AppDbContext _context): Repository<Session>(_context), ISessionsRepository
    {
        public async Task<Session> CreateNewSession(Session session, CancellationToken cancellationToken)
        {
            var sessionCreatedd = await _context.Sessions.AddAsync(session, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return sessionCreatedd.Entity;
        }
    }
}