using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Absences.v1.Queries
{
    public class GetAbsenceByIdQuery : BaseRequest, IRequest<AbsenceDetailDto?>
    {
        public Guid AbsenceId { get; set; }
    }
}
