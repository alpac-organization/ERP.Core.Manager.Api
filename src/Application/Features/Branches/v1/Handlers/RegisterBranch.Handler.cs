using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Handlers
{
    public class RegisterBranchHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<RegisterBranchHandler> _logger) : IRequestHandler<RegisterBranchCommand, bool>
    {
        public async Task<bool> Handle(RegisterBranchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚩Data inicial: \n\n {@request}", request);

            var company = await _unitOfWork.Companies.Entities
                .Where(company => company.Id == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if(company is null)
            {
                return _errorManager.ThrowBadRequest<bool>("¡Esta empresa no se encuentra registrada en nuestro sistema!", "ERP");
            }

            //Pendiente a implentar el metodo de insert al sistema.
            // await _unitOfWork.Branches.RegisterBranch(new()
            // {
                
            // });

            _logger.LogInformation("✅Sucursal registrada con exito");
            return true;
        }
    }
}