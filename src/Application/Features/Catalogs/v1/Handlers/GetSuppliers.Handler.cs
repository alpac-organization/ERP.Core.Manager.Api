using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetSuppliersHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) :  BaseValidatorHandler<GetSuppliersQuery,PagedResponse<SupplierDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponse<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            var suppliersQuery = _unitOfWork.Suppliers.Entities
                .Include(sup => sup.User)
                    .ThenInclude(user => user.WorkArea)
                .Where(sup => sup.IsActive)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                suppliersQuery = suppliersQuery
                    .Where(sup => sup.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.ConstitutionType.HasValue)
            {
                suppliersQuery = suppliersQuery
                    .Where(sup => sup.ConstitutionType == request.ConstitutionType);
            }

            var suppliers = await suppliersQuery
                .OrderByDescending(sup => sup.CreatedAt) 
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);


            var totalCount = await _unitOfWork.Suppliers.Entities
                .CountAsync(cancellationToken);

            var suppliersMapped = _mapper.Map<List<SupplierDto>>(suppliers);

            return new PagedResponse<SupplierDto>(
                suppliersMapped,
                request.PageNumber,
                request.PageSize,
                totalCount
            );
        }
    }
}