using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Database.Domain.Entities.Warehouse;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Validators;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class RegisterCustomerHandler(
        IUnitOfWork unitOfWork,
        IErrorManager errorManager,
        IMapper mapper,
        RegisterCustomerValidator validator,
        IS3StorageService s3StorageService)
        : BaseValidatorHandler<RegisterCustomerCommand, bool>(unitOfWork, errorManager)
    {
        public override async Task<bool> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
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

            // 3. Validar que el tipo de cliente exista
            var customerType = await _unitOfWork.CustomerType.Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(ct => ct.Id == request.CustomerTypeId && ct.DeletedAt == null, cancellationToken);

            if (customerType == null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "El tipo de cliente indicado no existe en el sistema.",
                    "ERP:CUSTOMER_TYPE_NOT_FOUND");
            }

            // 4. Validar que no exista un cliente con el mismo número de identificación
            var existingCustomer = await _unitOfWork.Customers.Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => 
                    c.IdentificationNumber == request.IdentificationNumber && 
                    c.CompanyId == request.CompanyId && 
                    c.DeletedAt == null, 
                    cancellationToken);

            if (existingCustomer != null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "Ya existe un cliente con este número de identificación.",
                    "ERP:CUSTOMER_ALREADY_EXISTS");
            }

            // 5. Validar que no exista un cliente con el mismo CIF
            existingCustomer = await _unitOfWork.Customers.Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => 
                    c.Cif == request.Cif && 
                    c.CompanyId == request.CompanyId && 
                    c.DeletedAt == null, 
                    cancellationToken);

            if (existingCustomer != null)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    "Ya existe un cliente con este CIF.",
                    "ERP:CUSTOMER_CIF_ALREADY_EXISTS");
            }

            // 6. Crear el cliente
            var customer = mapper.Map<Customer>(request);
            customer.IsActive = true;

            // 7. Subir imagen a S3 si viene en base64
            if (!string.IsNullOrWhiteSpace(request.PictureBase64))
            {
                var imageUrl = await s3StorageService.UploadImageAsync(
                    module: "manager",
                    section: "customers",
                    base64Image: request.PictureBase64,
                    cancellationToken);

                customer.PictureUrl = imageUrl;
            }

            await _unitOfWork.Customers.RegisterCustomer(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}