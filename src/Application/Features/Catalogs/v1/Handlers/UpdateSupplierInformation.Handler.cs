using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class UpdateSupplierInformationHandler(IUnitOfWork _unitOfWork, ILogger<UpdateSupplierInformationHandler> _logger, IErrorManager _errorManager) : BaseValidatorHandler<UpdateSupplierInformationCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(UpdateSupplierInformationCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                _logger.LogWarning("Usuario {UserId} (Supervisor) intentó actualizar proveedor {SupplierId} sin permiso", request.UserId, request.SupplierId);
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para registrar un proveedor", "ERP:01");
            }

            var user = await _unitOfWork.Users.Entities
                .Where(user => user.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Usuario desconocido!", "ERP:01");
            }

            var supplier = await _unitOfWork.Suppliers.Entities
                .Where(sup => sup.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<bool>("El registro de este proveedor no existe", "ERP:04");
            }

            if (access.Role!.RoleType != RoleType.Administrator && access.Role.RoleType != RoleType.Manager)
            {
                _logger.LogWarning("Usuario {UserId} con rol {RoleType} intentó actualizar proveedor {SupplierId} sin permiso", request.UserId, access.Role.RoleType, request.SupplierId);
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para actualizar este proveedor", "ERP:01");
            }

            supplier.ContactName          = request.ContactName          ?? supplier.ContactName;
            supplier.ContactEmail         = request.ContactEmail         ?? supplier.ContactEmail;
            supplier.SuppliersLegalName   = request.SuppliersLegalName   ?? supplier.SuppliersLegalName;
            supplier.ContactPhoneNumber   = request.ContactPhoneNumber   ?? supplier.ContactPhoneNumber;
            supplier.EmailSupport         = request.EmailSupport         ?? supplier.EmailSupport;
            supplier.Address              = request.Address              ?? supplier.Address;
            supplier.IdentificationNumber = request.IdentificationNumber ?? supplier.IdentificationNumber;
            supplier.ConstitutionType     = request.ConstitutionType     ?? supplier.ConstitutionType;
            supplier.IdentificationType   = request.IdentificationType   ?? supplier.IdentificationType;

            await _unitOfWork.Suppliers.UpdateAsync(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UserId} actualizó el proveedor {SupplierId}", request.UserId, request.SupplierId);

            return true;
        }
    }
}