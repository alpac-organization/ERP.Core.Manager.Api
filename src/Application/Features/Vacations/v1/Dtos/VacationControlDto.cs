using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos
{
    public class VacationControlDto
    {
        public decimal AmountDays { get; set; }
        public string? Description { get; set; }
        public string? WorkPosition { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? CollaboratorFullname { get; set;  }
        public PermitApplicationType PermitApplicationType { get; set; }
    }
}