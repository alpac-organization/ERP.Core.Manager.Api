using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Application.Commons.Mappings;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class RegisterCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager errorManager, ICodeGenerator codeGenerator, IMapper mapper)
    : AlpacBaseHandler<RegisterCollaboratorCommand, bool>(_unitOfWork, errorManager)
    {
        public override async Task<bool> Handle(RegisterCollaboratorCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar acceso (Seguridad centralizada en el BaseHandler)
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            var existsInCompany = await _unitOfWork.Collaborators.Entities
                .AnyAsync(c => c.IdentificationNumber == request.IdentificationNumber && c.CompanyId == request.CompanyId, cancellationToken);

            if (existsInCompany)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"El número de identificación {request.IdentificationNumber} ya está registrado en esta empresa.", 
                    "ERP:001"
                );
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken);

            var code = codeGenerator.GenerateModuleCode(request.IdentificationNumber!);
            request.RegisteredBy = user!.UserName;

            var collaboratorEntity = CollaboratorMapper.ToCollaboratorEntity(request, code);

            await _unitOfWork.Collaborators.RegisterCollaborator(collaboratorEntity,cancellationToken);


            if (request.PersonalInformation != null)
            {
                var personalInfo = request.PersonalInformation.ToPersonalInformationEntity(collaboratorEntity.Id);
                await _unitOfWork.PersonalInformations.RegisterPersonalInformation(personalInfo, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);


            if (access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Operator)
            {
                
            }

            return true;
        }
    }
}