using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Customers.v1.Handlers
{
    public class GetCustomersAvailableHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetCustomersAvailableQuery, List<CustomerDto>>
    {
        public async Task<List<CustomerDto>> Handle(GetCustomersAvailableQuery request, CancellationToken cancellationToken)
        {

            var customersQuery = _unitOfWork.Customers.Entities
                .Where(cus => cus.IsActive == request.Status)
                .Where(cus => cus.CompanyId == request.CompanyId)
                .AsNoTracking();

            if (request.CustomerTypeId.HasValue)
            {
                customersQuery = customersQuery
                    .Where(cus => cus.CustomerTypeId == request.CustomerTypeId);
            }

            var customers = await customersQuery
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<CustomerDto>>(customers);
        }
    }
}