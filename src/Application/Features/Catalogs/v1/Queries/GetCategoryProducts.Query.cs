using MediatR;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

public class GetCategoryProductsQuery : BaseRequest, IRequest<List<CategoryProductDto>>
{
    public Guid? ParentId { get; set; }
}