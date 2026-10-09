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

            var (linkError, productUnitMeasures) = await SupplierProductLinkValidator.ValidateProductsToLinkAsync(
                _unitOfWork,
                request.Products,
                alreadyLinkedProductIds: null,
                "ERP:ERROR_REGISTER",
                cancellationToken);

            if (linkError is not null)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>(linkError.Message, linkError.Code);
            }

            var resolvedSupplierType = request.SupplierType ?? request.SupplierDetails.SupplierType;
            if (!resolvedSupplierType.HasValue)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>(
                    "El tipo de proveedor es obligatorio (supplier_type o supplier_details.supplier_type).",
                    "ERP:ERROR_REGISTER");
            }

            request.SupplierDetails.SupplierType = resolvedSupplierType.Value;

            _logger.LogInformation(
                "Iniciando proceso de registro de proveedor. SupplierType={SupplierType}",
                resolvedSupplierType.Value);

            var now = DateTime.UtcNow;
            var supplierEntity = SupplierMapper.ToSupplierEntity(request, access.User.Fullname ?? "unknow user");

            supplierEntity.SupplierProducts = request.Products
                .Select(p => SupplierProductPriceHelper.BuildSupplierProduct(
                    supplierEntity.Id,
                    p.ProductId,
                    p.UnitPrice,
                    p.TierPrices,
                    p.UnitMeasureId ?? productUnitMeasures.GetValueOrDefault(p.ProductId),
                    now,
                    p.Currency ?? request.SupplierDetails.Currency))
                .ToList();

            await _unitOfWork.Suppliers.RegisterSupplier(supplierEntity);

            var supplierDetailsEntity = SupplierMapper.ToSupplierDetails(request.SupplierDetails, supplierEntity.Id);
            supplierDetailsEntity.SupplierType = resolvedSupplierType.Value;
            await _unitOfWork.SuppliersDetails.RegisterSupplierDetails(supplierDetailsEntity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registro de proveedor finalizado con exito");

            return new RegisterSupplierDto { SupplierId = supplierEntity.Id };
        }
    }
}
