using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

public class GetReasonsInventoryRemovalQuery : BaseRequest, IRequest<PagedResponse<ReasonsInventoryRemovalDto>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}