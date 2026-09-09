using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetSupplierBankAccountsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
        : BaseValidatorHandler<GetSupplierBankAccountsQuery, List<SupplierBankAccountDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<SupplierBankAccountDto>> Handle(GetSupplierBankAccountsQuery request, CancellationToken cancellationToken)
        {
            var supplier = await _unitOfWork.Suppliers.Entities
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.IsActive && s.Id == request.SupplierId)
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier is null)
            {
                return _errorManager.ThrowBadRequest<List<SupplierBankAccountDto>>("No se encontró registro de este proveedor", "ERP:NOT_FOUND");
            }

            var activeAccounts = supplier.SupplierBankAccounts
                .Where(b => b.DeletedAt == null)
                .ToList();

            return _mapper.Map<List<SupplierBankAccountDto>>(activeAccounts);
        }
    }
}
