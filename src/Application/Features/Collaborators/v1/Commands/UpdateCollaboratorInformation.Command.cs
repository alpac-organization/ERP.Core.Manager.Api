using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class UpdateCollaboratorInformationCommand: BaseRequest, IRequest<bool>
    {
        public string? IdentificationNumber { get; set; }
        public PInformation? PersonalInformation { get; set; } = new ();
        public WInformation? WorkingInformation { get; set; } = new();
    }

    public class PInformation
    {
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public string? Address { get; set; }
        public int? DepartamentId { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
    }

    public class WInformation
    {
        public string? WorkPhoneNumber { get; set; }
        public string? WorkEmail { get; set; }
        public string? InssNumber { get; set; }
        public string? BankAccountNumber { get; set; }

        //Solo por parte de administradores y manager pueden aplicar estos
        public int? WorkAreaId { get; set; }
        public int? WorkPositionId { get; set; }
        public int? BranchId { get; set; }
    }
}