using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Handlers
{
    public class GetTypesIncomeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetTypesIncomeAvailableQuery, List<TypesIncomeDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<TypesIncomeDto>> Handle(GetTypesIncomeAvailableQuery request, CancellationToken cancellationToken)
        {
            var typesIncome = await _unitOfWork.TypesIncome.Entities
                .Where(type => type.IsActive)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<TypesIncomeDto>>(typesIncome);
        }
    }
}
