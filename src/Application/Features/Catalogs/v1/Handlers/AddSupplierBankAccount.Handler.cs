using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class AddSupplierBankAccountHandler(IUnitOfWork _unitOfWork, ILogger<AddSupplierBankAccountHandler> _logger, IErrorManager _errorManager, IMapper _mapper)
        : BaseValidatorHandler<AddSupplierBankAccountCommand, SupplierBankAccountDto>(_unitOfWork, _errorManager)
    {
        public override async Task<SupplierBankAccountDto> Handle(AddSupplierBankAccountCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            if (access.Role?.RoleType == RoleType.Supervisor)
            {
                return _errorManager.ThrowBadRequest<SupplierBankAccountDto>("No tienes permiso para agregar cuentas bancarias a un proveedor", "ERP:01");
            }

            var supplier = await _unitOfWork.Suppliers.Entities
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.IsActive && s.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<SupplierBankAccountDto>("El proveedor especificado no existe", "ERP:NOT_FOUND");
            }

            var activeAccounts = supplier.SupplierBankAccounts
                .Where(b => b.DeletedAt == null)
                .ToList();

            bool isPrimary = request.IsPrimary || activeAccounts.Count == 0;
            if (isPrimary)
            {
                foreach (var acc in activeAccounts)
                {
                    acc.IsPrimary = false;
                }
            }

            var newAccount = new SupplierBankAccount
            {
                SupplierId = supplier.Id,
                BankName = request.BankName.Trim(),
                AccountNumber = request.AccountNumber.Trim(),
                AccountType = request.AccountType,
                Currency = request.Currency,
                AccountHolderName = request.AccountHolderName.Trim(),
                AccountHolderIdentification = request.AccountHolderIdentification?.Trim(),
                IsPrimary = isPrimary
            };

            supplier.SupplierBankAccounts.Add(newAccount);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario {UserId} agregó la cuenta bancaria {AccountId} al proveedor {SupplierId}", request.UserId, newAccount.Id, supplier.Id);

            return _mapper.Map<SupplierBankAccountDto>(newAccount);
        }
    }
}
