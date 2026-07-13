using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;

//Obtener solicitudes de vacaciones registradas durante el periodo de nomina
namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class GetPermitApplicationInPayrollHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetPermitApplicationInPayrollQuery, List<PermitApplicationDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<PermitApplicationDto>> Handle(GetPermitApplicationInPayrollQuery request, CancellationToken cancellationToken)
        {
            #region Evaluar acceso al modulo
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }
            #endregion Evaluar acceso al modulo

            //Query de busqueda.
            var recordsQuery = _unitOfWork.PermitApplications.Entities
                .Where(permit => permit.PayrolId == request.PayrollId)
                .Include(permit => permit.Collaborator)
                .AsNoTracking();

            //Primer filtro de la busqueda de permit applications realizadas
            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                recordsQuery = recordsQuery
                    .Where(permit => permit.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            //Segundo filtro de la busqueda de permit application realizada
            if (request.Status.HasValue)
            {
                recordsQuery = recordsQuery
                    .Where(permit => permit.Status == request.Status);
            }

            //Busqueda final de los permisos
            var records = await recordsQuery
                .OrderBy(permit => permit.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<PermitApplicationDto>>(records);
        }
    }
}   