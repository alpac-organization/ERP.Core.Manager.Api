using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetSupplierDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetSupplierDetailsQuery, SupplierInformationDto>(_unitOfWork, _errorManager)
    {
        public override async Task<SupplierInformationDto> Handle(GetSupplierDetailsQuery request, CancellationToken cancellationToken)
        {
            var suppliersQuery = await _unitOfWork.Suppliers.Entities
                .Include(sup => sup.SupplierDetails)
                .Include(sup => sup.User)
                    .ThenInclude(user => user.WorkArea)
                .Where(sup => sup.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
                


            return new ();
        }
    }
}