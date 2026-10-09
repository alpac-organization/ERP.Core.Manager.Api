using MediatR;
using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetPurchaseRejectionReasonsHandler(IUnitOfWork _unitOfWork, IErrorManager _erroManager, IMapper _mapper) : IRequestHandler<GetPurchaseRejectionReasonsQuery, List<CatalogDetailsDto>>
    {
        public async Task<List<CatalogDetailsDto>> Handle(GetPurchaseRejectionReasonsQuery request, CancellationToken cancellationToken)
        {
            var catalog = await _unitOfWork.CatalogsRepository.FirstOrDefaultAsync(c =>
                c.CatalogType == CatalogType.PurchaseRejectionReasons &&
                (c.IsGlobal || c.CompanyId == request.CompanyId),
                cancellationToken);

            if (catalog is null)
            {
                return _erroManager.ThrowBadRequest<List<CatalogDetailsDto>>(
                    "El catálogo solicitado no existe o no tiene permisos para verlo.",
                    "ERP:001");
            }

            var subCatalogs = await _unitOfWork.SubCatalogs.GetSubCatalogsByCatalogId(catalog.Id, cancellationToken);

            return _mapper.Map<List<CatalogDetailsDto>>(subCatalogs);
        }
    }
}
