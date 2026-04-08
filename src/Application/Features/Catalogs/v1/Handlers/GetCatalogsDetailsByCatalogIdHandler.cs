using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetCatalogsDetailsByCatalogIdHandler(IUnitOfWork _unitOfWork, IErrorManager _erroManager, IMapper _mapper) : IRequestHandler<GetCatalogsDetailsByCatalogIdQuery, List<CatalogDetailsDto>>
    {
        public async Task<List<CatalogDetailsDto>> Handle(GetCatalogsDetailsByCatalogIdQuery request, CancellationToken cancellationToken)
        {
            var catalog = await _unitOfWork.CatalogsRepository.FirstOrDefaultAsync(c => 
                c.CatalogType == request.CatalogType && 
                (c.IsGlobal || c.CompanyId == request.CompanyId), 
                cancellationToken);

            if (catalog is null)
            {
                return _erroManager.ThrowBadRequest<List<CatalogDetailsDto>>(
                    "El catálogo solicitado no existe o no tiene permisos para verlo.", 
                    "ERP:001");
            }

            // Una vez validado el acceso al padre, traemos los hijos (SubCatalogs)
            var subCatalogs = await _unitOfWork.SubCatalogs.GetSubCatalogsByCatalogId(catalog.Id, cancellationToken);

            return _mapper.Map<List<CatalogDetailsDto>>(subCatalogs);
        }
    }
}