using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Queries
{
    public class GetTypesIncomeAvailableQuery : BaseRequest, IRequest<List<TypesIncomeDto>>
    {
        
    }
}
