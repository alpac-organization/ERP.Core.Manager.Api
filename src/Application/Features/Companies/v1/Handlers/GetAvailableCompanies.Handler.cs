using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Application.Features.Companies.v1.Handlers
{
    public class GetAvailableCompaniesHandler(IUnitOfWork _unitOfWork, IMapper _mapper, ILogger<GetAvailableCompaniesHandler> _logger) : IRequestHandler<GetAvailableCompaniesQuery, List<CompanyDto>>
    {       
        public async Task<List<CompanyDto>> Handle(GetAvailableCompaniesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo empresas disponibles");

            var companies = await _unitOfWork.Companies.Entities
                .Where(comp => comp.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Empresas con exito ✅");

            return _mapper.Map<List<CompanyDto>>(companies);
        }
    }
}