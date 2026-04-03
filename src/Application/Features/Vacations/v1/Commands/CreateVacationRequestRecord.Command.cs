using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands
{
    public class CreateVacationRequestRecordCommand : BaseRequest, IRequest<bool>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}