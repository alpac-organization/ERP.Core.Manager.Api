using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

//Obtener solicitudes de vacaciones registradas durante el periodo de nomina
namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class GetTypesAccountingPayrollHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetTypesAccountingPayrollQuery, List<TypesAccountingPayrollDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<TypesAccountingPayrollDto>> Handle(GetTypesAccountingPayrollQuery request, CancellationToken cancellationToken)
        {
            #region Evaluar acceso al modulo
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }
            #endregion Evaluar acceso al modulo

            var records = await _unitOfWork.TypesAccountingPayroll.Entities
                .Where(type => type.IsActive)
                .Where(type => type.CompanyId == request.CompanyId)
                .ToListAsync(cancellationToken);


            return _mapper.Map<List<TypesAccountingPayrollDto>>(records);
        }
    }
}   