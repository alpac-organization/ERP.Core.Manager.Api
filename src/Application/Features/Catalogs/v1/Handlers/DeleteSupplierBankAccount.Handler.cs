using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class DeleteSupplierBankAccountHandler(IUnitOfWork _unitOfWork, ILogger<DeleteSupplierBankAccountHandler> _logger, IErrorManager _errorManager)
        : BaseValidatorHandler<DeleteSupplierBankAccountCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(DeleteSupplierBankAccountCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para eliminar cuentas bancarias de un proveedor", "ERP:01");
            }

            var lookup = await SupplierBankAccountHandlerHelper.FindSupplierAndAccountAsync(
                _unitOfWork,
                _errorManager,
                request.SupplierId,
                request.BankAccountId,
                cancellationToken);

            if (!lookup.IsSuccess)
            {
                return lookup.ErrorResult;
            }

            var supplier = lookup.Supplier!;
            var account = lookup.Account!;

            bool wasPrimary = account.IsPrimary;
            account.DeletedAt = DateTime.UtcNow;
            account.IsPrimary = false;

            if (wasPrimary)
            {
                var remainingPrimary = supplier.SupplierBankAccounts
                    .FirstOrDefault(b => b.Id != account.Id && b.DeletedAt == null);

                if (remainingPrimary != null)
                {
                    remainingPrimary.IsPrimary = true;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UserId} eliminó la cuenta bancaria {AccountId} del proveedor {SupplierId}", request.UserId, account.Id, supplier.Id);

            return true;
        }
    }
}
