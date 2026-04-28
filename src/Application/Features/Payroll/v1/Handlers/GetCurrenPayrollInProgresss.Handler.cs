using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class GetCurrenPayrollInProgresssHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<GetCurrenPayrollInProgresssQuery, PayrollDto>(_unitOfWork, _errorManager)
    {
        public override async Task<PayrollDto> Handle(GetCurrenPayrollInProgresssQuery request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            //Solo Administradores puede realizar la apertura de la nomina.

            var payroll = await _unitOfWork.Payrolls.Entities
                .AsNoTracking()
                .Include(p => p.Branch)
                    .ThenInclude(branch => branch.Company)
                .Where( 
                    p => p.Branch.Company.Id == request.CompanyId && 
                    p.PayrollType == request.Type && 
                    p.Status == PayrollStatus.Progress &&
                    p.Branch.Id == request.BranchId
                )
                .Select(p => new PayrollDto
                {
                    PayrollId  = p.Id,
                    StartDate  = p.StartDate,
                    EndDate    = p.EndDate ?? p.StartDate.AddDays(14),
                    Type       = p.PayrollType,
                    BranchName = p.Branch.BranchName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll == null)
            {
                return _errorManager.ThrowNotFound<PayrollDto>("No se encontró una nómina en progreso para este tipo", "ERP:404");
            }

            var detailsQuery = _unitOfWork.OrdinaryPayrolls.Entities
                .AsNoTracking()
                .Where(op => op.PayrollId == payroll.PayrollId);

            int totalItems = await detailsQuery.CountAsync(cancellationToken);

            var pagedItems = await detailsQuery
                .OrderBy(op => op.Collaborator.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(op => new PyrollDtailsDto
                {
                    OrdinaryPayrollId = op.Id,
                    BiweeklySalary = op.BiweeklySalary,

                    NumberOfOvertime = op.NumberOfOvertime,
                    Overtime = op.Overtime,
                    Bonus = op.Bonus,
                    GrossSalary = op.GrossSalary,

                    Ir = op.Ir,
                    Inss = op.Inss,
                    TotalLegalDeductions = op.TotalLegalDeductions,

                    DeductionsAdditionalData = op.DeductionsAdditionalData,
                    TotalDeducctions = op.TotalDeducctions,
                    
                    TravelExpenses = op.TravelExpenses,
                    FoodTravelAllowance = op.FoodTravelAllowance,
                    Lodging = op.Lodging,

                    TotalToPay = op.TotalToPay,
                    Vacations = op.Vacations,

                    Collaborator = new CollaboratorInformationDto
                    {
                        FullName = $"{op.Collaborator.FirstName} {op.Collaborator.SecondName ?? ""} {op.Collaborator.FirstLastname} {op.Collaborator.SecondLastname ?? ""}",
                        CollaboratorCode = op.Collaborator.CollaboratorCode,
                        IdentificationNumber = op.Collaborator.IdentificationNumber,
                        InssNumber = op.Collaborator.WorkingInformation.InssNumber,
                        JobPosition = op.Collaborator.WorkingInformation.WorkPosition.CatalogName,
                        WorkArea = op.Collaborator.WorkingInformation.WorkArea.CatalogName
                    }
                })
                .ToListAsync(cancellationToken);

            payroll.PayrollDetails = new PaginatedDetailsDto
            {
                Items = pagedItems,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return payroll;
        }
    }
}   