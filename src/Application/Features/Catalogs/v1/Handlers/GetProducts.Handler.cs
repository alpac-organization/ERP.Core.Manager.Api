using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetProductsHandler(IUnitOfWork unitOfWork, IErrorManager errorManager, IMapper _mapper) : BaseValidatorHandler<GetProductsQuery, List<ProductDto>>(unitOfWork, errorManager)
    {
        public override async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(
                request.UserId,
                request.CompanyId,
                request.ModuleCode,
                cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }
            var query = _unitOfWork.Products.Entities
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(c => !c.DeletedAt.HasValue);

            if (request.ProductId.HasValue)
                query = query.Where(c => c.Id == request.ProductId.Value);

            var products = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<ProductDto>>(products);
        }
    }
}
