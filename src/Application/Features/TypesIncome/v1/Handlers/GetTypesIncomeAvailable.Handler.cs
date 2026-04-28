using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Handlers
{
   public class GetTypesIncomeHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetTypesIncomeAvailableQuery, List<TypesIncomeDto>>
    {
        public async Task<List<TypesIncomeDto>> Handle(GetTypesIncomeAvailableQuery request, CancellationToken cancellationToken)
        {

            var typesIncome = await _unitOfWork.TypesIncome.Entities
                .Where(type => type.IsActive)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<TypesIncomeDto>>(typesIncome);
        }
    } 
}