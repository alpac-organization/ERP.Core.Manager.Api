using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands
{
    public class CreatePermitApplicationCommand : BaseRequest, IRequest<bool>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public string? Description { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}