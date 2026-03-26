using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands
{
    public class CreateModuleAssociatedWithCompanyCommand : IRequest<bool>
    {
        public string? ModuleName { get; set; }
        public Guid CompanyId { get; set; }
        public string? Description { get; set; }
        
    }
}