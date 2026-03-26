using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using AutoMapper;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class RegisterCollaboratorHandler(
        IUnitOfWork unitOfWork, 
        IErrorManager errorManager, 
        ICodeGenerator codeGenerator,
        IMapper mapper)
        : AlpacBaseHandler<RegisterCollaboratorCommand, bool>(unitOfWork, errorManager)
    {
        public override async Task<bool> Handle(RegisterCollaboratorCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar acceso (Seguridad centralizada en el BaseHandler)
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var code = codeGenerator.GenerateModuleCode(request.IdentificationNumber!);
            request.Code = code;

            var collaboratorData = mapper.Map<Collaborator>(request);

            switch (access.Role!.RoleType)
            {
                case RoleType.Administrator:
                {
                    await unitOfWork.Collaborators.RegisterCollaborator(collaboratorData, cancellationToken);
                    
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    break;
                }
                default:
                    return _errorManager.ThrowBadRequest<bool>("No tienes permisos de administrador para esta acción", "ERP:007");
            }

            return true;
        }
    }
}