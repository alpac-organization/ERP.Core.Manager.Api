using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using Microsoft.Extensions.Logging;


namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
   public class InitializePayrollProcessHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IPayrollServices _payrollServices, ILogger<InitializePayrollProcessHandler> _logger) : AlpacBaseHandler<InitializePayrollProcessCommand, bool>(_unitOfWork, _errorManager)
   {
      public override async Task<bool> Handle(InitializePayrollProcessCommand request, CancellationToken cancellationToken)
      {
         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         if (access.Role!.RoleType != RoleType.Administrator)
         {
            return _errorManager.ThrowBadRequest<bool>("Solo los administradores pueden aperturar el ciclo de la nomina", "ERP:001");
         }

         var branch = await _unitOfWork.Branches.Entities
             .Where(branch =>
                 branch.Id == request.BranchId && branch.CompanyId == request.CompanyId
             )
             .Include(branch => branch.Company)
             .FirstOrDefaultAsync(cancellationToken);

         if (branch is null)
         {
            return _errorManager.ThrowBadRequest<bool>("La sucursal seleccionada no estas asociado a este compañia", "ERP:BrachNotFound");
         }

         var payrollInProgress = await _unitOfWork.Payrolls.Entities
            .Where(payroll => payroll.BranchId == request.BranchId)
            .Include(payroll => payroll.Branch)
               .ThenInclude(branch => branch.Company)
            .Where(payroll => payroll.Branch.Company.Id == request.CompanyId)
            .Where(payroll => payroll.Status == PayrollStatus.Progress)
            .Where(payroll => payroll.PayrollType == request.Type)
            .AnyAsync(cancellationToken);

         if (payrollInProgress)
         {
            return _errorManager.ThrowBadRequest<bool>("No se puede aperturar mientras exista un nomina en progreso", "ERP:01");
         }

         var lastPayroll = await _unitOfWork.Payrolls.Entities
            .Where(payroll => payroll.BranchId == request.BranchId)
            .Include(payroll => payroll.Branch)
               .ThenInclude(branch => branch.Company)
            .Where(payroll => payroll.Branch.Company.Id == request.CompanyId)
            .Where(payroll => payroll.Status == PayrollStatus.Closed)
            .Where(payroll => payroll.PayrollType == request.Type)
            .OrderByDescending(payroll => payroll.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

         //Definir las fechas de apertura de la nomina. 
         var (startDate, endDate, period) = ManagerUtils.DefineRegularPayrollOpeningDates(lastPayroll);

         var newPayroll = new Database.Domain.Entities.Payrolls.Payroll()
         {
            Id          = Guid.NewGuid(),
            StartDate   = startDate,
            EndDate     = endDate,
            Period      = period,
            PayrollType = request.Type,
            BranchId    = request.BranchId,
            Status      = PayrollStatus.Progress,
         };

         //Crear nuevo registro de nomina en progreso
         await _unitOfWork.Payrolls.InitializePayroll(newPayroll);

         switch (request.Type)
         {
            case PayrollType.Ordinary:
            {
               var collaborators = await _payrollServices.ObtainsCollaboratorByType(SalaryType.Fixed, request.CompanyId, request.BranchId);


               foreach (var collaborator in collaborators)
               {
                  bool isRegistered = await _payrollServices.RegisterCollaboratorToPayroll(newPayroll, collaborator);

                  if(!isRegistered)
                  {
                     _logger.LogWarning("No se pudo registrar al colaborador {CollaboratorId} en la nomina {PayrollId}", collaborator.IdentificationNumber, newPayroll.Id);
                  }
               }

               await _unitOfWork.SaveChangesAsync(cancellationToken);

               return true;
            }

            case PayrollType.ProfessionalServices:
               {
                  var collaborators = await _payrollServices.ObtainsCollaboratorByType(SalaryType.ProfessionalServices, request.CompanyId, request.BranchId);

                  switch (branch.Company.Alias)
                  {
                     case "VIGEMSA":
                        {
                           foreach (var collaborator in collaborators)
                           {
                              await _payrollServices.RegisterCollaboratorToVigemsaProfessional(newPayroll.Id, collaborator);
                           }

                           break;
                        }
                  }

                  //Guardar cambios de nominas prestacionadas seccionadas
                  await _unitOfWork.SaveChangesAsync(cancellationToken);
                  return true;
               }
            default:
               {
                  return _errorManager.ThrowBadRequest<bool>("Error la crear esta nomina, el tipo de nomina no es valido", "ERP:01");
               }
         }
      }
   }
}