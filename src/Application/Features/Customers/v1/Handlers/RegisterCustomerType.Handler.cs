using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Validators;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class RegisterCustomerTypeHandler(
        IUnitOfWork unitOfWork,
        IErrorManager errorManager,
        IMapper mapper,
        RegisterCustomerTypeValidator validator)
        : BaseValidatorHandler<RegisterCustomerTypeCommand, bool>(unitOfWork, errorManager)
    {
        public override async Task<bool> Handle(RegisterCustomerTypeCommand request, CancellationToken cancellationToken)
        {
            // 1. Validación de acceso
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode, cancellationToken);
            if (!access.IsSuccess) return access.ErrorResponse!;

            // 2. Validación de datos
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    validation.Errors.First().ErrorMessage,
                    "ERP:VALIDATION_ERROR");
            }

            // 3. Validar que no exista un tipo de cliente con el mismo código
            var existingCustomerTypeByCode = await _unitOfWork.CustomerType.Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.Code == request.Code && ct.DeletedAt == null, cancellationToken);

            if (existingCustomerTypeByCode != null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "Ya existe un tipo de cliente con este código.",
                    "ERP:CUSTOMER_TYPE_CODE_ALREADY_EXISTS");
            }

            // 4. Validar que no exista un tipo de cliente con el mismo nombre
            var existingCustomerTypeByName = await _unitOfWork.CustomerType.Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.Name.ToLower() == request.Name.ToLower() && ct.DeletedAt == null, cancellationToken);

            if (existingCustomerTypeByName != null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "Ya existe un tipo de cliente con este nombre.",
                    "ERP:CUSTOMER_TYPE_NAME_ALREADY_EXISTS");
            }

            // 5. Crear el tipo de cliente
            var customerType = mapper.Map<CustomerType>(request);
            customerType.IsActive = true;

            await _unitOfWork.CustomerType.RegisterCustomerType(customerType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}