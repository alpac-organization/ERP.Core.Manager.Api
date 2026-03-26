using MediatR;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Users.v1.Queries
{
    /// <summary>
    /// Consulta para obtener la colección de usuarios activos asociados a una empresa específica.
    /// </summary>
    /// <remarks>
    /// Esta consulta filtra los usuarios a través de sus perfiles vinculados, 
    /// garantizando que solo se retornen aquellos con acceso vigente al Tenant solicitado.
    /// </remarks>
    /// <param name="companyId">Identificador único de la empresa para realizar el filtrado de usuarios.</param>
    public class GetAllActiveUsersByCompanyIdQuery(Guid companyId) : IRequest<List<UserDto>> 
    {
        /// <summary>
        /// Identificador de la empresa (CompanyId) proporcionado para la consulta.
        /// </summary>
        public Guid CompanyId { get; } = companyId;
    }
}