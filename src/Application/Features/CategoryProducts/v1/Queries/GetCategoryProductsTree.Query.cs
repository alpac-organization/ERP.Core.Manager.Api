using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Queries;

public class GetCategoryProductsTreeQuery : BaseRequest, IRequest<List<CategoryProductDto>>
{
    public Guid? ParentId { get; set; }
}