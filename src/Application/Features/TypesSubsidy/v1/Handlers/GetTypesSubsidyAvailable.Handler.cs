using MediatR;
using AutoMapper;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Handlers
{
   public class GetTypesSubsidyHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetTypesSubsidyAvailableQuery, List<TypeSubsidyDto>>
    {
        public async Task<List<TypeSubsidyDto>> Handle(GetTypesSubsidyAvailableQuery request, CancellationToken cancellationToken)
        {

            return [];
        }
    } 
}