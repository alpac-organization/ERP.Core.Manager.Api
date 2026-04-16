using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll
{
    public class CollaboratorsRepository(AppDbContext _context): Repository<Collaborator>(_context), ICollaboratorsRepository
    {
        public async Task<Collaborator> RegisterCollaborator(Collaborator collaborator, CancellationToken cancellationToken)
        {
            collaborator.Status = CollaboratorStatus.Active;
            collaborator.PictureUrl = null;

            var collaboratorRegistered = await _context.Collaborators.AddAsync(collaborator, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);

            return collaboratorRegistered.Entity;
        }
    }
}