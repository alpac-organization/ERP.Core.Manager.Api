using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Companies.v1.Handlers
{
    public class GetAvailableCompaniesHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetAvailableCompaniesQuery, List<CompanyDto>>
    {       
        public async Task<List<CompanyDto>> Handle(GetAvailableCompaniesQuery request, CancellationToken cancellationToken)
        {
            var companies = await _unitOfWork.Companies.GetAvailableCompanies(cancellationToken);
            return _mapper.Map<List<CompanyDto>>(companies);
        }
    }
}