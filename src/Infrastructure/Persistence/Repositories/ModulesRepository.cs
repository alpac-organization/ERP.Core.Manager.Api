using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories
{
    public class ModulesRepository(AppDbContext _context): Repository<Module>(_context), IModulesRepository
    {
        public async Task<List<Module>> ObtainActiveModulesByCompanyId(int CompanyId, CancellationToken cancellationToken)
        {
            return await _context.Modules
                .Where(module => module.IsActive == true && module.CompanyId == CompanyId)
                .OrderBy(module => module.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task CreateModuleAssociatedWithCompany(Module Payload, CancellationToken cancellationToken)
        {
            //Creamos el registro en la base de datos.
            await _context.Modules.AddAsync(Payload, cancellationToken);

            //Guardamos los cambios.
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}