using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class GetCustomersAvailableHandler(IUnitOfWork unitOfWork, IErrorManager errorManager) : BaseValidatorHandler<GetCustomersAvailableQuery, List<CustomerDto>>(unitOfWork, errorManager)
    {
        public override async Task<List<CustomerDto>> Handle(GetCustomersAvailableQuery request, CancellationToken cancellationToken)
        {
            // 1. Validación de acceso
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            } 

            // 3. Construir query base
            var customersQuery = _unitOfWork.Customers.Entities
                .AsNoTracking()
                .Where(cus => cus.CompanyId == request.CompanyId)
                .Where(cus => cus.DeletedAt == null);

            // 4. Filtro por estado
            if (request.Status.HasValue)
            {
                customersQuery = customersQuery.Where(cus => cus.IsActive == request.Status.Value);
            }

            //Agregar filtro aqui, terminar endpoint

            return [];
        }
    }
}