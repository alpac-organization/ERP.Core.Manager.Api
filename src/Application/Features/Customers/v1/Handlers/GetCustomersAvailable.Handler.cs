using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class GetCostCenterByAreaHandler(IUnitOfWork _unitOfWork /*, IErrorManager _errorManager, IMapper _mapper */) : IRequestHandler<GetCustomersAvailableQuery, List<CustomerDto>>
    {
        public async Task<List<CustomerDto>> Handle(GetCustomersAvailableQuery request, CancellationToken cancellationToken)
        {
            var customers = await _unitOfWork.Customers.Entities
                .Where(cus => cus.IsActive)
                .ToListAsync(cancellationToken);

                

            return [];
        }
    }
}