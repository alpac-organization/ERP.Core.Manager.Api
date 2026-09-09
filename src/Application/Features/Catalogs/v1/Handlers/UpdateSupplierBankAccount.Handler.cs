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
    public class UpdateSupplierBankAccountHandler(IUnitOfWork _unitOfWork, ILogger<UpdateSupplierBankAccountHandler> _logger, IErrorManager _errorManager)
        : BaseValidatorHandler<UpdateSupplierBankAccountCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(UpdateSupplierBankAccountCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes permiso para modificar cuentas bancarias de un proveedor", "ERP:01");
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

            account.BankName = request.BankName?.Trim() ?? account.BankName;
            account.AccountNumber = request.AccountNumber?.Trim() ?? account.AccountNumber;
            account.AccountType = request.AccountType ?? account.AccountType;
            account.Currency = request.Currency ?? account.Currency;
            account.AccountHolderName = request.AccountHolderName?.Trim() ?? account.AccountHolderName;
            account.AccountHolderIdentification = request.AccountHolderIdentification?.Trim() ?? account.AccountHolderIdentification;

            if (request.IsPrimary.HasValue && request.IsPrimary.Value)
            {
                foreach (var acc in supplier.SupplierBankAccounts.Where(b => b.DeletedAt == null))
                {
                    acc.IsPrimary = false;
                }
                account.IsPrimary = true;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UserId} actualizó la cuenta bancaria {AccountId} del proveedor {SupplierId}", request.UserId, account.Id, supplier.Id);

            return true;
        }
    }
}
