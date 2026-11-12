using MediatR;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

public class GetCategoryProductsQuery : BaseRequest, IRequest<List<CategoryProductDto>>
{
    public CategoryType? CategoryType { get; set; }
}