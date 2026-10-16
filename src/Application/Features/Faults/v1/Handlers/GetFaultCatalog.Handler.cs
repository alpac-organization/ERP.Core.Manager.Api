using AutoMapper;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Handlers
{
   public class GetFaultCatalogHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
      : IRequestHandler<GetFaultCatalogQuery, PagedResponse<FaultCatalogDto>>
   {
      public async Task<PagedResponse<FaultCatalogDto>> Handle(GetFaultCatalogQuery request, CancellationToken cancellationToken)
      {
         return new PagedResponse<FaultCatalogDto>(
            [], request.PageNumber, request.PageSize
         );
      }
   }
}