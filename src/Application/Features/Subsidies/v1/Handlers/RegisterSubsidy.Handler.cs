using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Handlers
{
    public class RegisterSubsidyHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IIncomeServices _incomeServices): AlpacBaseHandler<RegisterSubsidyCommmand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterSubsidyCommmand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            var typeSubsidy = await _unitOfWork.TypesSubsidies.Entities
                .Where(type => type.IsActive)
                .Where(type => type.Id == request.TypeSubsidyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (typeSubsidy is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El tipo de subsidio seleccionado no existe", "ERP:NotFound");
            }

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Where(collaborator => collaborator.Id == request.CollaboratorId) 
                .Where(collaborator => collaborator.Status != CollaboratorStatus.Inactive)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaboratorInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este collaborador no fue encontrado", "ERP:NotFound");
            }

            var salaryInformation = await _unitOfWork.Salaries.Entities
                .Where(salary => salary.EndDate == null)
                .Where(salary => salary.SalaryType == SalaryType.Fixed)
                .Where(salary => salary.CollaboratorId == collaboratorInformation.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (salaryInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la información salarial del colaborador", "ERP:NotFound");
            }

            var payrollActive = await _unitOfWork.Payrolls.Entities
                .Where(pay => pay.Id == request.PayrollId)
                .Where(pay => pay.Status == PayrollStatus.Progress)
                .FirstOrDefaultAsync(cancellationToken);

            if (payrollActive is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se puede iniciar el proceso de subsidio si no se encuentrea una nomina activa", "ERP:BadRequest");
            }


            switch (typeSubsidy.Code)
            {
                case "COMMON_ILLNESS":
                {
                    await _incomeServices.ApplyMedicalSubsidy(collaboratorInformation, salaryInformation, payrollActive, request);

                    return true;
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Este tipo de subsidio no se encuetra en funcionamiento.", "ERP:BadRequest");   
                }
            }
        }
    }
}
