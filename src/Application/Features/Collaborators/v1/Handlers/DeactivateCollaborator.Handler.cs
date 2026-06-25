using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class DeactivateCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<UpdateCollaboratorInformationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(UpdateCollaboratorInformationCommand request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Where(col => col.CompanyId == request.CompanyId)
                .Where(col => col.IdentificationNumber == request.IdentificationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este colaborador no existe en nuestro sitema,", "ERP:01");
            }

            //Verificar que tipo de salario tiene para saber donde ir a sacarlo de que nomina
            

            return true;
        }
    }
}