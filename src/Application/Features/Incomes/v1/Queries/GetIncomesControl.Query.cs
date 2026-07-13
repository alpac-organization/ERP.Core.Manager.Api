using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Queries
{
    public class GetIncomeControlQuery : IRequest<PagedResponse<IncomesDto>>
    {
        
    }
}