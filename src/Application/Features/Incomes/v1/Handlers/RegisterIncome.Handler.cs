using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Handlers
{
    public class RegisterDeductionHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger logger): AlpacBaseHandler<RegisterIncomeCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(RegisterIncomeCommand request, CancellationToken cancellationToken)
        {

            #pragma warning disable CA1873 // Avoid potentially expensive logging
            
            logger.LogInformation("data: {data}", JsonSerializer.Serialize(request));
            
            #pragma warning restore CA1873 // Avoid potentially expensive logging

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar una dedución", "ERP:01");
            }

            logger.LogInformation("Iniciando proceso de registro de ingreso");

            var collaboratorInformation = await _unitOfWork.Collaborators.Entities
                .Where(col => col.IdentificationNumber == request.IdentificationNumber && col.CompanyId == request.CompanyId)
                .Include(col => col.WorkingInformation)
                    .ThenInclude(work => work.BranchInfo)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaboratorInformation is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este collaborador no existe", "ERP:01");
            }

            //Obtener la nomina del la sucursal a la que pertence
            var payrollOrdinary = await _unitOfWork.Payrolls.Entities 
                .Where(pay => pay.Status == PayrollStatus.Progress && pay.BranchId == collaboratorInformation.WorkingInformation.BranchInfo.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (payrollOrdinary is null)
            {
                logger.LogInformation("No se encontro una nomina en progreso");
                return false;
            }
            else
            {
                //Verificarel si ese ingreso esta disponible
                var Income = await _unitOfWork.TypesIncome.Entities
                    .Where(type => type.Id == request.TypeIncomeId && type.IsActive)
                    .FirstOrDefaultAsync(cancellationToken);

                if (Income is null)
                {
                    // return _
                }

                //Iniciando proceso de ingreso y contabilidad de nomina.
                // switch(request.)




            }

            return true; 
        }
    }
}