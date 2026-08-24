using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Validators;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class GetCustomersAvailableHandler(
        IUnitOfWork unitOfWork,
        IErrorManager errorManager,
        GetCustomersAvailableValitor validator)
        : BaseValidatorHandler<GetCustomersAvailableQuery, List<CustomerDto>>(unitOfWork, errorManager)
    {
        public override async Task<List<CustomerDto>> Handle(GetCustomersAvailableQuery request, CancellationToken cancellationToken)
        {
            // 1. Validación de acceso
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);
            if (!access.IsSuccess) return access.ErrorResponse!;

            // 2. Validación de datos
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return _errorManager.ThrowBadRequest<List<CustomerDto>>(
                    validation.Errors.First().ErrorMessage,
                    "ERP:VALIDATION_ERROR");
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

            // 5. Filtro por tipo de cliente
            if (request.CustomerTypeId.HasValue)
            {
                customersQuery = customersQuery.Where(cus => cus.CustomerTypeId == request.CustomerTypeId.Value);
            }

            // 6. Ordenar y ejecutar
            var customers = await customersQuery
                .OrderBy(cus => cus.LegalName)
                .Select(cus => new CustomerDto
                {
                    CustomerId = cus.Id,
                    LegalName = cus.LegalName,
                    IdentificationNumber = cus.IdentificationNumber,
                    IdentificationType = cus.IdentificationType
                })
                .ToListAsync(cancellationToken);

            return customers;
        }
    }
}