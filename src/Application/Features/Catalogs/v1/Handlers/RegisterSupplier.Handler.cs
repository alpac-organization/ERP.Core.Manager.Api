using Microsoft.Extensions.Logging;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;


namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class RegisterSupplierHandler(IUnitOfWork _unitOfWork, ILogger<RegisterSupplierHandler> _logger, IErrorManager _errorManager) : BaseValidatorHandler<RegisterSupplierCommand, RegisterSupplierDto>(_unitOfWork, _errorManager)
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

            _logger.LogInformation("🚀Iniciando proceso de registro de proveedor");

            var supplierEntity = SupplierMapper.ToSupplierEntity(request, access.User.Fullname ?? "unknow user");

            await _unitOfWork.Suppliers.RegisterSupplier(supplierEntity);

            var supplierDetailsEntity = SupplierMapper.ToSupplierDetails(request.SupplierDetails, supplierEntity.Id);

            if (request.SupplierDetails.HasCredit && request.SupplierDetails.CreditDays < 1)
            {
                return _errorManager.ThrowBadRequest<RegisterSupplierDto>("Los dias de creditos deben contener almenos un dia", "ERP:ERROR_REGISTER");
            }

            await _unitOfWork.SuppliersDetails.RegisterSupplierDetails(supplierDetailsEntity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Registro finalizado con exito");

            return new () { SupplierId = supplierDetailsEntity.Id };
        }
   }
}