using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands
{
    public class CreateModuleAssociatedWithCompanyCommand : IRequest
    {
        public string? ModuleName { get; set; }
        public int CompanyId { get; set; }
        public string? Description { get; set; }
        
    }
}