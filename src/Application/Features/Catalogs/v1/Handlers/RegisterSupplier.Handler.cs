using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Helpers;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class RegisterSupplierHandler(
        IUnitOfWork _unitOfWork,
        ILogger<RegisterSupplierHandler> _logger,
        IErrorManager _errorManager)
        : BaseValidatorHandler<RegisterSupplierCommand, RegisterSupplierDto>(_unitOfWork, _errorManager)
    {
        public override async Task<RegisterSupplierDto> Handle(RegisterSupplierCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>("No tienes permiso para registrar un proveedor", "ERP:01");
            }

            if (request.BankAccounts.Count(b => b.IsPrimary) > 1)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>("Solo se puede definir una cuenta bancaria como principal", "ERP:ERROR_REGISTER");
            }

            if (request.SupplierDetails.ExclusiveStatus is not SupplierExclusiveStatus.None
                and not SupplierExclusiveStatus.PendingReview)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>(
                    "Al registrar un proveedor solo se permite ExclusiveStatus None o PendingReview.",
                    "ERP:ERROR_REGISTER");
            }

            if (request.SupplierDetails.HasCredit && request.SupplierDetails.CreditDays < 1)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>("Los dias de creditos deben contener almenos un dia", "ERP:ERROR_REGISTER");
            }

            if (request.Products.Count > 0)
            {
                var productIds = request.Products.Select(p => p.ProductId).Distinct().ToList();

                if (productIds.Count != request.Products.Count)
                {
                    return _errorManager.ThrowBadRequest<RegisterSupplierDto>(
                        "No se puede relacionar el mismo producto más de una vez al proveedor.",
                        "ERP:ERROR_REGISTER");
                }

                var existingProductCount = await _unitOfWork.Products.Entities
                    .CountAsync(p => productIds.Contains(p.Id) && p.DeletedAt == null, cancellationToken);

                if (existingProductCount != productIds.Count)
                {
                    return _errorManager.ThrowBadRequest<RegisterSupplierDto>(
                        "Uno o más productos seleccionados no existen.",
                        "ERP:ERROR_REGISTER");
                }

                foreach (var productItem in request.Products)
                {
                    var overlapError = SupplierProductPriceHelper.ValidateTierPriceOverlaps(
                        productItem.TierPrices ?? []);

                    if (overlapError is not null)
                    {
                        return _errorManager.ThrowBadRequest<RegisterSupplierDto>(overlapError, "ERP:ERROR_REGISTER");
                    }
                }
            }

            _logger.LogInformation("Iniciando proceso de registro de proveedor");

            var now = DateTime.UtcNow;
            var supplierEntity = SupplierMapper.ToSupplierEntity(request, access.User.Fullname ?? "unknow user");

            supplierEntity.SupplierProducts = request.Products
                .Select(p => SupplierProductPriceHelper.BuildSupplierProduct(
                    supplierEntity.Id,
                    p.ProductId,
                    p.UnitPrice,
                    p.Currency,
                    p.TierPrices,
                    unitMeasureId: null,
                    now))
                .ToList();

            await _unitOfWork.Suppliers.RegisterSupplier(supplierEntity);

            var supplierDetailsEntity = SupplierMapper.ToSupplierDetails(request.SupplierDetails, supplierEntity.Id);
            await _unitOfWork.SuppliersDetails.RegisterSupplierDetails(supplierDetailsEntity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registro de proveedor finalizado con exito");

            return new RegisterSupplierDto { SupplierId = supplierEntity.Id };
        }
    }
}
