using MediatR;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands
{
    public class CreatePermitApplicationCommand : BaseRequest, IRequest<bool>
    {
        public string? Description { get; set; }
        public PermitApplicationType PermitApplicationType { get; set; }
    
        public PermitApplicationVacation? PermitApplicationVacation { get; set; }



        [JsonIgnore]
        public string? IdentificationNumber { get; set; }
    }

    public class PermitApplicationVacation
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PermitApplicationDonatedVacations
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? IdentificationCollaboratorToReceive { get; set; }
    }
}