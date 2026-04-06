using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Enums;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class UpdateCollaboratorInformationCommand: BaseRequest, IRequest<bool>
    {
        public string? IdentificationNumber { get; set; }
        public PInformation? PersonalInformation { get; set; } 
        public WInformation? WorkingInformation { get; set; }
    }

    public class PInformation
    {
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Departament { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
    }

    public class WInformation
    {
        public string? WorkPhoneNumber { get; set; }
        public string? WorkEmail { get; set; }
        public string? InssNumber { get; set; }
        public string? BankAccountNumber { get; set; }

        //Solo por parte de administradores y manager pueden aplicar estos
        public int WorkAreaId { get; set; }
        public virtual SubCatalog WorkArea { get; set; } = null!;
        public int WorkPositionId { get; set; }
        public virtual SubCatalog WorkPosition { get; set; } = null!;
        public int BranchId { get; set; }
        public virtual SubCatalog Branch { get; set; } = null!;
    }
}