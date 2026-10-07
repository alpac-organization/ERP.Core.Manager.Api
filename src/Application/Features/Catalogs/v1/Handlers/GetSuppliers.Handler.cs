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
    public class GetSuppliersHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper)
        : BaseValidatorHandler<GetSuppliersQuery, PagedResponse<SupplierDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponse<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var suppliersQuery = _unitOfWork.Suppliers.Entities
                .Include(sup => sup.User)
                .Include(sup => sup.SupplierPaymentMethods)
                .Include(sup => sup.SupplierDetails)
                .Where(sup => sup.IsActive && sup.DeletedAt == null)
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

            if (!string.IsNullOrWhiteSpace(request.CommercialName))
            {
                suppliersQuery = suppliersQuery
                    .Where(sup =>
                        sup.CommercialName != null &&
                        sup.CommercialName.Contains(request.CommercialName));
            }

            if (request.ExclusiveStatus.HasValue)
            {
                suppliersQuery = suppliersQuery
                    .Where(sup =>
                        sup.SupplierDetails != null &&
                        sup.SupplierDetails.ExclusiveStatus == request.ExclusiveStatus.Value);
            }

            var totalCount = await suppliersQuery.CountAsync(cancellationToken);

            var suppliers = await suppliersQuery
                .OrderByDescending(sup => sup.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

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
