using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Queries
{
    public class ObtainActiveModulesByCompanyIdQuery : IRequest<List<ModuleDto>>
    {
        public int CompanyId { get; set; }
    }
}