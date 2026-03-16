using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;

namespace ERP.Core.Manager.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CompaniesController(IMediator _mediator) : ApiControllerBase
    {
        [HttpGet("companies")]       
        public async Task<List<CompanyDto>> GetAvailableCompaniesAsync()
        {
            return await _mediator.Send(
                new GetAvailableCompaniesQuery()
            );
        }
    }
}
