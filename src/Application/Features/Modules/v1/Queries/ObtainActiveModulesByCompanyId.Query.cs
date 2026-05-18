using MediatR;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Queries
{
    public class ObtainActiveModulesByCompanyIdQuery : IRequest<List<ModuleDto>>
    {
        public Guid CompanyId { get; set; }
    }
}