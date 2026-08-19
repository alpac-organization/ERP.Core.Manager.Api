using AutoMapper;
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
    public class GetCustomerTypesHandler(
        IUnitOfWork unitOfWork,
        IErrorManager errorManager,
        IMapper mapper,
        GetCustomerTypesValidator validator)
        : BaseValidatorHandler<GetCustomerTypesQuery, List<CustomerTypeDto>>(unitOfWork, errorManager)
    {
        public override async Task<List<CustomerTypeDto>> Handle(GetCustomerTypesQuery request, CancellationToken cancellationToken)
        {
            // 1. Validación de acceso
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);
            if (!access.IsSuccess) return access.ErrorResponse!;

            // 2. Validación de datos
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return _errorManager.ThrowBadRequest<List<CustomerTypeDto>>(
                    validation.Errors.First().ErrorMessage,
                    "ERP:VALIDATION_ERROR");
            }

            // 3. Construir query base
            var customerTypesQuery = _unitOfWork.CustomerType.Entities
                .AsNoTracking()
                .Where(ct => ct.DeletedAt == null);

            // 4. Filtro por estado
            if (request.Status.HasValue)
            {
                customerTypesQuery = customerTypesQuery.Where(ct => ct.IsActive == request.Status.Value);
            }

            // 5. Ordenar y ejecutar
            var customerTypes = await customerTypesQuery
                .OrderBy(ct => ct.Name)
                .ToListAsync(cancellationToken);

            // 6. Mapear y retornar
            return mapper.Map<List<CustomerTypeDto>>(customerTypes);
        }
    }
}