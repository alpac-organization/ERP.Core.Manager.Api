using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries
{
    public class GetWorkAreasQuery: BaseRequest, IRequest<List<WorkAreaDto>>
    {
        
    }
}