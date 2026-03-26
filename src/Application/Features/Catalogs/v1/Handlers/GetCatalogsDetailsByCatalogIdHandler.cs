using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetCatalogsDetailsByCatalogIdHandler(IUnitOfWork _unitOfWork, IErrorManager _erroManager, IMapper _mapper) : IRequestHandler<GetCatalogsDetailsByCatalogIdQuery, List<CatalogDetailsDto>>
    {
        public async Task<List<CatalogDetailsDto>> Handle(GetCatalogsDetailsByCatalogIdQuery request, CancellationToken cancellationToken)
        {
            var catalog = await _unitOfWork.Catalogs.FirstOrDefaultAsync(catalog => catalog.CompanyId == request.CompanyId && catalog.CatalogType == request.CatalogType, cancellationToken);

            if (catalog is null)
            {
                return _erroManager.ThrowBadRequest<List<CatalogDetailsDto>>("Este tipo de catalogo no esta disponible para esta empresa", "ERP:001");
            }

            var subCatalogs = await _unitOfWork.SubCatalogs.GetSubCatalogsByCatalogId(catalog!.Id, cancellationToken);

            //Mapper
            return [];
        }
    }
}