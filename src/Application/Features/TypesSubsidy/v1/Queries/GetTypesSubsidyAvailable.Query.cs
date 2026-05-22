using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Queries
{
    public class GetTypesSubsidyAvailableQuery : BaseRequest, IRequest<List<TypeSubsidyDto>>
    {
        
    }
}
