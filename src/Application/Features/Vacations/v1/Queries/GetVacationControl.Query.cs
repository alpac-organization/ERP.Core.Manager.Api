using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries
{
    public class GetVacationControl : BaseRequest, IRequest<List<VacationControlDto>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}