using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class UpdateCollaboratorInformationCommand: IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string? ModuleCode { get; set; }
        public string? IdentificationNumber { get; set; }

    }

    public class PInformation
    {
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Departament { get; set; }
    }

}