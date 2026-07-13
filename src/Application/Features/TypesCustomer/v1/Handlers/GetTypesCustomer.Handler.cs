using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.TypesCustomer.v1.Handlers
{
    public class GetTypesCustomerHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetTypesCustomerQuery, List<TypeCustomerDto>>
    {
        public async Task<List<TypeCustomerDto>> Handle(GetTypesCustomerQuery request, CancellationToken cancellationToken)
        {

            return [];
        }
    }
}