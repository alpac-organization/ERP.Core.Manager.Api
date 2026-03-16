using MediatR;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Companies.v1.Queries
{
    /// <summary>
    /// Consulta para obtener la lista de empresas disponibles.
    /// </summary>
    public class GetAvailableCompaniesQuery : IRequest<List<CompanyDto>> {}
}